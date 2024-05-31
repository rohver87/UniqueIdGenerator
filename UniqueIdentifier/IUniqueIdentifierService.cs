using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqueIdentifier
{
    public interface IUniqueIdentifierService
    {
        ulong GenerateUniqueId();
        IdComponents GetIdComponentsFromGeneratedId(ulong id);
    }
}
