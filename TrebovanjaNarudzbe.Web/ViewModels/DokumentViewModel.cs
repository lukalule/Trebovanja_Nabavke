using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TrebovanjaNarudzbe.Web.ViewModels
{
    public class DokumentViewModel : HttpPostedFileBase
    {
        public int? DokumentId { get; set; }
        public string Naziv { get; set; }
        public readonly byte[] fileBytes;

        public DokumentViewModel() { }
        public DokumentViewModel(byte[] fileBytes, string fileName = null, int? dokumentId = null)
        {
            this.DokumentId = dokumentId;
            this.fileBytes = fileBytes;
            this.FileName = fileName;
            this.InputStream = new MemoryStream(fileBytes);
        }        

        public override int ContentLength => fileBytes.Length;
        public override string FileName { get; }
        public override Stream InputStream { get; }

    }
}