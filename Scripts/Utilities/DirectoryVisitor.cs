#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;

namespace OnionCollections.DataEditor
{
    internal class DirectoryVisitor
    {
        string path;

        const char splitChar = '/';

        internal DirectoryVisitor(string path)
        {
            path = path.Replace('\\', splitChar);

            if (IsFolder(path) == false)
            {
                path = path.Substring(0, path.LastIndexOf(splitChar) + 1);
            }

            this.path = path.Replace('\\', splitChar);
        }

        internal DirectoryVisitor Enter(string folderName)
        {
            if (IsFolder(path) == false)
                throw new System.Exception("Path is not a folder.");

            path += folderName + splitChar;
            return this;
        }

        internal DirectoryVisitor Back()
        {
            if (path[path.Length - 1] != splitChar)
                throw new System.Exception("Path is not a folder.");

            path = path.Substring(0, path.Remove(path.Length - 1).LastIndexOf(splitChar) + 1);
            return this;
        }

        internal bool HasFolder(string folderName)
        {
            string checkPath = new DirectoryVisitor(path).Enter(folderName).GetPathWithoutSplitChar();
            return AssetDatabase.IsValidFolder(checkPath);
        }

        internal DirectoryVisitor CreateFolder(string folderName)
        {
            AssetDatabase.CreateFolder(GetPathWithoutSplitChar(), folderName);
            //Debug.Log($"Create Folder:{GetPathWithoutSplitChar()} , {folderName}");

            return this;
        }

        internal DirectoryVisitor CreateFolderIfNotExist(string folderName)
        {
            if (HasFolder(folderName) == false)
                CreateFolder(folderName);

            return this;
        }

        internal string GetPathWithoutSplitChar()
        {
            return path.Remove(path.Length - 1);
        }

        public override string ToString()
        {
            return path;
        }

        static bool IsFolder(string path)
        {
            return path[path.Length - 1] == splitChar;
        }


    }
}
#endif
