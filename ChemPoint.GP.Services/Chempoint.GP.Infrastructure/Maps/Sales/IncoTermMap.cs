using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class IncoTermMap : BaseDataTableMap<IncoTerm>
    {
        public override IncoTerm Map(DataRow dr)
        {
            IncoTerm incoTerm = new IncoTerm();
            incoTerm.IncoTerms = dr["IncoTermCode"].ToString().Trim();
            incoTerm.IncoTermsDescription = dr["IncoTermDescription"].ToString().Trim();
            return incoTerm;
        }
    }
}
