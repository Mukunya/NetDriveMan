using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;

namespace VBase
{
#if WINDOWS
    public class FileBrowser
    {				
        private string _lastPath;
        private string _extension;
        private string _hint;

        public enum EFileOperations { Open, Save, ChooseFolder }

        public FileBrowser(string Extension, string Hint)
        {
            _lastPath = "";
            _extension = Extension;
            _hint = Hint;
        }

        public string GetFileName(EFileOperations FileOperation)
        {
            switch (FileOperation)
            {
                default:
                    OpenFileDialog dlg = new OpenFileDialog();


                    dlg.DefaultExt = _extension;

                    dlg.Filter = _hint + "|*" + _extension;

                    dlg.CheckFileExists = true;

                    if (_lastPath.Length > 2)
                        dlg.InitialDirectory = _lastPath;


                    Nullable<bool> result = dlg.ShowDialog();

                    string fileName = null;

                    if (result == true)
                    {
                        fileName = dlg.FileName;
                        _lastPath = Path.GetDirectoryName(fileName);
                    }

                    return fileName;
                case EFileOperations.Save:
                    SaveFileDialog dlg1 = new SaveFileDialog();

                    dlg1.DefaultExt = _extension;

                    dlg1.Filter = _hint + "|*" + _extension;

                    dlg1.CheckFileExists = false;

                    if (_lastPath.Length > 2)
                        dlg1.InitialDirectory = _lastPath;

                    bool? result1 = dlg1.ShowDialog();

                    string fname = null;

                    //if (result1==null)
                    //{
                        fname = dlg1.FileName;
                        _lastPath = Path.GetDirectoryName(fname);
                    //}

                    return fname;
                case EFileOperations.ChooseFolder:
                    OpenFileDialog dlg2 = new OpenFileDialog();


                    dlg2.DefaultExt = _extension;

                    dlg2.Filter = _hint + "|*" + _extension;

                    dlg2.CheckFileExists = false;
                    dlg2.CheckPathExists = true;
                    dlg2.FileName = "Choose folder";

                    if (_lastPath.Length > 2)
                        dlg2.InitialDirectory = _lastPath;


                    bool? result2 = dlg2.ShowDialog();

                    string fileName2 = null;

                    if (result2 == true)
                    {
                        fileName2 = Path.GetDirectoryName(dlg2.FileName);
                        _lastPath = fileName2;
                    }

                    return fileName2;
            }
            
        }
    }
#endif
}