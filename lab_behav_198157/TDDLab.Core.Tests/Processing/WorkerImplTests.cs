using System;
using Moq;
using TDDLab.Core.Infrastructure;
using TDDLab.Core.InvoiceMgmt;
using static TDDLab.Core.Tests.Processing.InvoiceBuilder;

namespace TDDLab.Core.Tests.Processing
{
    public class WorkerImplTests
    {
        private readonly Mock<IConfigurationSettings> configurationSettings = new Mock<IConfigurationSettings>();
        private readonly Mock<IMessagingFacility<Invoice, ProcessingResult>> messagingFacility = new Mock<IMessagingFacility<Invoice, ProcessingResult>>();
        private readonly Mock<IExceptionHandler> exceptionHandler = new Mock<IExceptionHandler>();
        private readonly Mock<IInvoiceProcessor> invoiceProcessor = new Mock<IInvoiceProcessor>();
        private readonly WorkerImpl worker;

        public WorkerImplTests()
        {
            worker = new WorkerImpl(
                configurationSettings.Object,
                messagingFacility.Object,
                exceptionHandler.Object,
                invoiceProcessor.Object);
        }

        private Message<Invoice> SetupInputMessage()
        {
            var inputMessage = new Message<Invoice>
            {
                Data = ValidInvoice(new InvoiceLine("Mleko", new Money(10))),
                Metadata = new Metadata().FromString("correlation-id")
            };
            messagingFacility.Setup(m => m.ReadMessage()).Returns(inputMessage);
            return inputMessage;
        }

        [Fact]
        public void Start_InitializesInputChannelWithConfiguredQueue()
        {
            configurationSettings.Setup(c => c.GetSettingsByKey("inputQueue")).Returns("in-q");

            worker.Start();

            messagingFacility.Verify(m => m.InitializeInputChannel("in-q"), Times.Once);
        }

        [Fact]
        public void Start_InitializesOutputChannelWithConfiguredQueue()
        {
            configurationSettings.Setup(c => c.GetSettingsByKey("outputQueue")).Returns("out-q");

            worker.Start();

            messagingFacility.Verify(m => m.InitializeOutputChannel("out-q"), Times.Once);
        }

        [Fact]
        public void Start_InitializesInputChannelBeforeOutputChannel()
        {
            var calls = new System.Collections.Generic.List<string>();
            messagingFacility.Setup(m => m.InitializeInputChannel(It.IsAny<string>())).Callback(() => calls.Add("input"));
            messagingFacility.Setup(m => m.InitializeOutputChannel(It.IsAny<string>())).Callback(() => calls.Add("output"));

            worker.Start();

            Assert.Equal(new[] { "input", "output" }, calls);
        }

        [Fact]
        public void Start_DoesNotReadOrDispose()
        {
            worker.Start();

            messagingFacility.Verify(m => m.ReadMessage(), Times.Never);
            messagingFacility.Verify(m => m.Dispose(), Times.Never);
        }

        [Fact]
        public void Stop_DisposesMessagingFacility()
        {
            worker.Stop();

            messagingFacility.Verify(m => m.Dispose(), Times.Once);
        }

        [Fact]
        public void DoJob_ProcessesInvoiceFromInputMessage()
        {
            var inputMessage = SetupInputMessage();

            worker.DoJob();

            invoiceProcessor.Verify(p => p.Process(inputMessage.Data), Times.Once);
        }

        [Fact]
        public void DoJob_WritesProcessingResultWithOriginalMetadata()
        {
            var inputMessage = SetupInputMessage();
            var processingResult = ProcessingResult.Succeeded();
            invoiceProcessor.Setup(p => p.Process(inputMessage.Data)).Returns(processingResult);

            worker.DoJob();

            messagingFacility.Verify(m => m.WriteMessage(It.Is<Message<ProcessingResult>>(msg =>
                msg.Data == processingResult && msg.Metadata == inputMessage.Metadata)), Times.Once);
        }

        [Fact]
        public void DoJob_WhenEverythingSucceeds_DoesNotCallExceptionHandler()
        {
            SetupInputMessage();
            invoiceProcessor.Setup(p => p.Process(It.IsAny<Invoice>())).Returns(ProcessingResult.Succeeded());

            worker.DoJob();

            exceptionHandler.Verify(h => h.HandleException(It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public void DoJob_WhenReadingFails_HandlesExceptionAndSkipsProcessing()
        {
            var exception = new InvalidOperationException("queue unavailable");
            messagingFacility.Setup(m => m.ReadMessage()).Throws(exception);

            worker.DoJob();

            exceptionHandler.Verify(h => h.HandleException(exception), Times.Once);
            invoiceProcessor.Verify(p => p.Process(It.IsAny<Invoice>()), Times.Never);
            messagingFacility.Verify(m => m.WriteMessage(It.IsAny<Message<ProcessingResult>>()), Times.Never);
        }

        [Fact]
        public void DoJob_WhenProcessingFails_HandlesExceptionAndDoesNotWrite()
        {
            SetupInputMessage();
            var exception = new Exception("processing error");
            invoiceProcessor.Setup(p => p.Process(It.IsAny<Invoice>())).Throws(exception);

            worker.DoJob();

            exceptionHandler.Verify(h => h.HandleException(exception), Times.Once);
            messagingFacility.Verify(m => m.WriteMessage(It.IsAny<Message<ProcessingResult>>()), Times.Never);
        }

        [Fact]
        public void DoJob_WhenWritingFails_HandlesException()
        {
            SetupInputMessage();
            invoiceProcessor.Setup(p => p.Process(It.IsAny<Invoice>())).Returns(ProcessingResult.Failed());
            var exception = new TimeoutException("write timeout");
            messagingFacility.Setup(m => m.WriteMessage(It.IsAny<Message<ProcessingResult>>())).Throws(exception);

            worker.DoJob();

            exceptionHandler.Verify(h => h.HandleException(exception), Times.Once);
        }

        [Fact]
        public void DoJob_WhenExceptionOccurs_DoesNotPropagateIt()
        {
            messagingFacility.Setup(m => m.ReadMessage()).Throws(new Exception());

            var thrown = Record.Exception(() => worker.DoJob());

            Assert.Null(thrown);
        }
    }
}
