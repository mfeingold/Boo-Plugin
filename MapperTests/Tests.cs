using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapperTests
{
    public class Tests
    {
        public static MapperTest[] MapperTests = 
        { 
            new MapperTest(
                    new Source(
"Test1",

@"a=1
print a"
,4
                        )
                )                                         
        };
    }
}
