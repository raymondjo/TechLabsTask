//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()]
namespace CRMEarlyBoundClasses.EarlyBoundedClasses
{
    /// <summary>
    /// Represents a source of entities bound to a CRM service. It tracks and manages changes made to the retrieved entities.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.71")]
    public partial class CrmServiceContext : Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CrmServiceContext(Microsoft.Xrm.Sdk.IOrganizationService service): base(service)
        {
        }

        /// <summary>
        /// Gets a binding to the set of all <see cref="CRMEarlyBoundClasses.EarlyBoundedClasses.Contact"/> entities.
        /// </summary>
        public System.Linq.IQueryable<CRMEarlyBoundClasses.EarlyBoundedClasses.Contact> ContactSet
        {
            get
            {
                return this.CreateQuery<CRMEarlyBoundClasses.EarlyBoundedClasses.Contact>();
            }
        }
    }
}