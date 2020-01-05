using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace STIKS.Server.Protocol
{
    public class ProtocolTools
    {
        public static byte[] ZipCompress(byte[] data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = zipArchive.CreateEntry("zip");

                    using (var entryStream = demoFile.Open())
                    {
                        entryStream.Write(data, 0, data.Length);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public static byte[] ZipDecompress(byte[] data)
        {
            try
            {
                using (var zippedStream = new MemoryStream(data))
                {
                    using (var archive = new ZipArchive(zippedStream))
                    {
                        var entry = archive.Entries.FirstOrDefault();

                        if (entry != null)
                        {
                            using (var unzippedEntryStream = entry.Open())
                            {
                                using (var ms = new MemoryStream())
                                {
                                    unzippedEntryStream.CopyTo(ms);

                                    return ms.ToArray();
                                }
                            }
                        }

                        return null;
                    }
                }

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
