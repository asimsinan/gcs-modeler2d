using System;

namespace FuzzyMsc.Dto
{
    public class ResultDTO
    {

        public string Message { get; set; }

    
        public bool Success { get; set; }


        public object ResultObject { get; set; }

   
        public Exception Exception { get; set; }
    }
}
