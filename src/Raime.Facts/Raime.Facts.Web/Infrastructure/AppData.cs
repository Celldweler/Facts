using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raime.Facts.Web.Infrastructure
{
    public class AppData
    {
        public const string AdminRoleName = "Administrator";
        public const string UserRoleName = "User";

        public static IEnumerable<string> Roles
        {
            get
            {
                yield return UserRoleName;
                yield return AdminRoleName;
            }
        }
            
    }
}
