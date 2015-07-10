using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class MetadataReaderDLL
{
    public static System.Reflection.Metadata.MetadataReader CreateFromPEReader(System.Reflection.PortableExecutable.PEReader pereader)
    {
        unsafe
        {
            var metadata = pereader.GetMetadata();
            return new System.Reflection.Metadata.MetadataReader(metadata.Pointer, metadata.Length);
        }
    }

}