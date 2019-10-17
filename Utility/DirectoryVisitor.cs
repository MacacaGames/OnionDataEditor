#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;

public class DirectoryVisitor
{
    string path;

    const char splitChar = '/';

    public DirectoryVisitor(string path)
    {
        path = path.Replace('\\', splitChar);

        if (IsFolder(path) == false)
        {
            path = path.Substring(0, path.LastIndexOf(splitChar) + 1);
        }

        this.path = path.Replace('\\', splitChar);
    }

    public DirectoryVisitor Enter(string folderName)
    {
        if (IsFolder(path) == false)
            throw new System.NotImplementedException("Path is not a folder.");

        path += folderName + splitChar;
        return this;
    }
    
    public DirectoryVisitor Back()
    {
        if (path[path.Length - 1] != splitChar)
            throw new System.NotImplementedException("Path is not a folder.");

        path = path.Substring(0, path.LastIndexOf(splitChar, 0, 2));
        return this;
    }

    public bool HasFolder(string folderName)
    {
        string checkPath = new DirectoryVisitor(path).Enter(folderName).ToString();
        return AssetDatabase.IsValidFolder(checkPath);
    }

    public DirectoryVisitor Create(string folderName)
    {
        AssetDatabase.CreateFolder(GetPathWithoutSplitChar(), folderName);
        Debug.Log($"Create Folder:{GetPathWithoutSplitChar()} , {folderName}");

        return this;
    }

    public string GetPathWithoutSplitChar()
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
#endif
