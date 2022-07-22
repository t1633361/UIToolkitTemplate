using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Achieve.Encrypt;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achieve.UI
{
    public class GUIEncrypt : MonoBehaviour
    {
        private ListView _fileListView;
        private ListView _infoListView;
        private Button   _refresh;
        private Button   _encrypt;
        private Button   _decrypt;
        private Button   _toSource;
        private Button   _openFile;
        private TextElement    _sourcePath;
        private TextElement    _encryptPath;
        private Button   _openSourceFolder;
        private Button   _openDestinationFolder;
        
        private VisualElement _uiEncrypt;
        
        private List<string> _fileList;
        private List<string> _infoList;
        
        
        private string _sourceFolder  = "./Source";
        private string _destinationFolder = "./Destination";
        private void Awake()
        {
            InitUI();
            InitFold();
            InitFileListView();
            InitInfoListView();
        }

        void InitUI()
        {
            _uiEncrypt        = GetComponent<UIDocument>().rootVisualElement;
            _fileListView     = _uiEncrypt.Q<ListView>("FileListView");
            _infoListView     = _uiEncrypt.Q<ListView>("InfoListView");
            
            _refresh = _uiEncrypt.Q<Button>("Refresh");
            _encrypt = _uiEncrypt.Q<Button>("Encrypt");
            _decrypt = _uiEncrypt.Q<Button>("Decrypt");
            _toSource = _uiEncrypt.Q<Button>("ToSource");
            _openFile = _uiEncrypt.Q<Button>("OpenFile");

            _sourcePath  = _uiEncrypt.Q<TextElement>("SourcePath");
            _encryptPath = _uiEncrypt.Q<TextElement>("DestinationPath");

            _openSourceFolder  = _uiEncrypt.Q<Button>("OpenSourceFolder");
            _openDestinationFolder = _uiEncrypt.Q<Button>("OpenDestinationFolder");
            
            _openSourceFolder.RegisterCallback<MouseUpEvent>((e)=>OpenFolder(_sourceFolder));
            _openDestinationFolder.RegisterCallback<MouseUpEvent>((e)=>OpenFolder(_destinationFolder));
            
            _refresh.RegisterCallback<MouseUpEvent>(Refresh);
            _encrypt.RegisterCallback<MouseUpEvent>(Encrypt);
            _decrypt.RegisterCallback<MouseUpEvent>(Decrypt);
            _toSource.RegisterCallback<MouseUpEvent>(ToSource);
            _openFile.RegisterCallback<MouseUpEvent>(OpenFile);
        }

        void InitFileListView()
        {
            RefreshFileList();
            
            _fileListView.makeItem      = MakeItem;
            _fileListView.bindItem      = BindFileListViewItem;
            _fileListView.itemsSource   = _fileList;
            _fileListView.selectionType = SelectionType.Multiple;
        }
        
        void InitInfoListView()
        {
            _infoList                   = new List<string>();
            _infoListView.makeItem      = MakeItem;
            _infoListView.bindItem      = BindInfoListViewItem;
            _infoListView.itemsSource   = _infoList;
            _infoListView.selectionType = SelectionType.Multiple;
        }


        void RefreshFileList()
        {
            DirectoryInfo sourceInfo = new DirectoryInfo(_sourceFolder);
            
            if(!sourceInfo.Exists)
                throw new DirectoryNotFoundException(_sourceFolder);

            var files = sourceInfo.GetFiles();
            
            if(_fileList == null)
                _fileList = new List<string>();
            else
                _fileList.Clear();
            
            foreach (var file in files)
            {
                _fileList.Add(file.FullName);
            }
        }
        
        private VisualElement MakeItem()
        {
            VisualElement container  = new VisualElement { name = "FilePathLine" };
            Label         filePath  = new Label { name         = "FilePath" };

            filePath.AddToClassList("Label");

            container.Add(filePath);

            return container;
        }

        private void BindFileListViewItem(VisualElement ve, int index)
        {
            var filePath = ve.Q<Label>("FilePath");
            filePath.text = Path.GetFileName(_fileList[index]);
        }
        
        private void BindInfoListViewItem(VisualElement ve, int index)
        {
            var filePath = ve.Q<Label>("FilePath");
            filePath.text = _infoList[index];
        }

        void InitFold()
        {
            if (!Directory.Exists(_sourceFolder))
            {
                Directory.CreateDirectory(_sourceFolder);
            }
            if (!Directory.Exists(_destinationFolder))
            {
                Directory.CreateDirectory(_destinationFolder);
            }
            DirectoryInfo sourceInfo = new DirectoryInfo(_sourceFolder);
            DirectoryInfo destinationInfo = new DirectoryInfo(_destinationFolder);
            _sourceFolder     = sourceInfo.FullName.Replace("\\", "/");
            _destinationFolder    = destinationInfo.FullName.Replace("\\", "/");
            _sourcePath.text  = _sourceFolder;
            _encryptPath.text = _destinationFolder;

        }
        private void OpenFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            Application.OpenURL("file:///" +  path);
        }

        private void DisableButtons()
        {
            _refresh.SetEnabled(false);
            _encrypt.SetEnabled(false);
            _decrypt.SetEnabled(false);
            _toSource.SetEnabled(false);
            _openFile.SetEnabled(false);
            _openSourceFolder.SetEnabled(false);
            _openDestinationFolder.SetEnabled(false);
        }

        private void EnableButtons()
        {
            _refresh.SetEnabled(true);
            _encrypt.SetEnabled(true);
            _decrypt.SetEnabled(true);
            _toSource.SetEnabled(true);
            _openFile.SetEnabled(true);
            _openSourceFolder.SetEnabled(true);
            _openDestinationFolder.SetEnabled(true);
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="evt"></param>
        private void Encrypt(MouseUpEvent evt)
        {
            try
            {
                ShowInfo("Encrypt...");
                DisableButtons();

                if (Directory.Exists(_destinationFolder))
                {
                    Directory.Delete(_destinationFolder,true);
                }

                Directory.CreateDirectory(_destinationFolder);

                var select = _fileListView.selectedIndices;

                int count = 0;
                foreach (var index in select)
                {
                    if (index >= _fileList.Count)
                    {
                        ShowErrorInfo(string.Format("Index is invalid:{0}  FileCount:{1}", index, _fileList.Count()));
                        continue;
                    }
                    if (!File.Exists(_fileList[index]))
                    {
                        ShowErrorInfo(string.Format("File is invalid:{0}", _fileList[index]));
                        continue;
                    }

                    EncryptSelectFile(_fileList[index]);
                    ++count;
                }

                ShowWarningInfo(string.Format("Encrypt Success:{0}",count));
            }
            catch (Exception e)
            {
                ShowErrorInfo(e.Message);
            }
            finally
            {
                EnableButtons();
            }
            
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="evt"></param>
        private void Decrypt(MouseUpEvent evt)
        {
            try
            {
                ShowInfo("Decrypt...");
                DisableButtons();

                if (Directory.Exists(_destinationFolder))
                {
                    Directory.Delete(_destinationFolder, true);
                }

                Directory.CreateDirectory(_destinationFolder);

                var select = _fileListView.selectedIndices;

                int count = 0;
                foreach (var index in select)
                {
                    if (index >= _fileList.Count)
                    {
                        ShowErrorInfo(string.Format("Index is invalid:{0}  FileCount:{1}", index, _fileList.Count()));
                        continue;
                    }
                    if (!File.Exists(_fileList[index]))
                    {
                        ShowErrorInfo(string.Format("File is invalid:{0}", _fileList[index]));
                        continue;
                    }

                    DecryptSelectFile(_fileList[index]);
                    ++count;
                }

                ShowWarningInfo(string.Format("Decrypt Success:{0}", count));
            }
            catch (Exception e)
            {
                ShowErrorInfo(e.Message);
            }
            finally
            {
                EnableButtons();
            }
            
        }

        void OpenFile(MouseUpEvent evt)
        {
            try
            {
                ShowInfo("OpenFile...");
                DisableButtons();

                var select = _fileListView.selectedIndices;

                int count = 0;
                foreach (var index in select)
                {
                    if (index >= _fileList.Count)
                    {
                        ShowErrorInfo(string.Format("Index is invalid:{0}  FileCount:{1}", index, _fileList.Count()));
                        continue;
                    }
                    if (!File.Exists(_fileList[index]))
                    {
                        ShowErrorInfo(string.Format("File is invalid:{0}", _fileList[index]));
                        continue;
                    }

                    System.Diagnostics.Process.Start(_fileList[index]);
                    ++count;
                }

                ShowWarningInfo(string.Format("OpenFile Success:{0}", count));
            }
            catch (Exception e)
            {
                ShowErrorInfo(e.Message);
            }
            finally
            {
                EnableButtons();
            }
        }

        void ToSource(MouseUpEvent evt)
        {
            try
            {
                ShowInfo("ToSource...");
                DisableButtons();

                if (!Directory.Exists(_sourceFolder))
                {
                    Directory.CreateDirectory(_sourceFolder);
                }

                var select = _fileListView.selectedIndices;

                int count = 0;
                foreach (var index in select)
                {
                    if (index >= _fileList.Count)
                    {
                        ShowErrorInfo(string.Format("Index is invalid:{0}  FileCount:{1}", index, _fileList.Count()));
                        continue;
                    }
                    if (!File.Exists(_fileList[index]))
                    {
                        ShowErrorInfo(string.Format("File is invalid:{0}", _fileList[index]));
                        continue;
                    }

                    var fileName = Path.GetFileName(_fileList[index]);

                    var destinationFile = $"{_destinationFolder}/{fileName}";

                    if (!File.Exists(destinationFile))
                    {
                        ShowWarningInfo($"文件不存在：{destinationFile}");
                    }
                    else
                    {
                        if (File.Exists(_fileList[index]))
                        {
                            File.Delete(_fileList[index]);
                        }
                        File.Move(destinationFile,_fileList[index]);   
                    }

                    ++count;
                }

                ShowWarningInfo(string.Format("ToSource Success:{0}", count));
            }
            catch (Exception e)
            {
                ShowErrorInfo(e.Message);
            }
            finally
            {
                EnableButtons();
            }
        }
        
        void EncryptSelectFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ShowErrorInfo($"File is invalid:{filePath}");
                return;
            }

            var filename        = Path.GetFileNameWithoutExtension(filePath);
            var extension       = Path.GetExtension(filePath);

            var encryptFilePath = $"{_destinationFolder}/{filename}{extension}";

            EncryptAsset.Encrypt(filePath, encryptFilePath);

            ShowInfo($"Encrypt success:{filePath.Replace("\\", "/")}");
        }
        void DecryptSelectFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ShowErrorInfo($"File is invalid:{filePath}");
                return;
            }

            var filename  = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            var encryptFilePath = $"{_destinationFolder}/{filename}{extension}";

            DecryptAsset.Decrypt(filePath, encryptFilePath);

            ShowInfo($"Decrypt success:{filePath.Replace("\\", "/")}");
        }
        

        private void ShowErrorInfo(string info)
        {
            PrintLog(info, 2);
        }
        private void ShowWarningInfo(string info)
        {
            PrintLog(info,1);
        }
        private void ShowInfo(string info)
        {
            PrintLog(info);
        }

        private void PrintLog(string info, int type=0)
        {
            string log = string.Format("{0} Info --- {1}", DateTime.Now, info);
            switch (type)
            {
                case 0:
                    Debug.Log(log);
                    break;
                case 1: 
                    log = string.Format("<color=yellow>{0}</color>", log);
                    Debug.LogWarning(log);
                    break;
                case 2: 
                    log = string.Format("<color=red>{0}</color>", log);
                    Debug.LogError(log);
                    break;
            }
            _infoList.Add(log);
            _infoListView.Rebuild();
        }

        private void Refresh(MouseUpEvent evt)
        {
            RefreshFileList();
            _fileListView.Rebuild();
        }
    }

}
