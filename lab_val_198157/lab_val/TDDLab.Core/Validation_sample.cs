namespace TDDLab.Core.InvoiceMgmt
{
//  Each ValidatedDomainObject implements following interface
//    public interface IValidatedObject
//    {
//        bool IsValid { get; }
//        IEnumerable<IRule> Validate();
//    }
//  Method Validate returns list of rules broken.

//  BusinessRule has Equality implemented based on rule name:
//
//        public override bool Equals(object obj)
//        {
//            IBusinessRule<T> other = obj as IBusinessRule<T>;
//            return (other == null ? true : this.Name.Equals(other.Name));
//        }
//
//    Example of checking if rule is ok
//    var invoiceHasProperRecipient = new Invoice().Validate().Contains(Invoice.ValidationRules.Recipient);

}
