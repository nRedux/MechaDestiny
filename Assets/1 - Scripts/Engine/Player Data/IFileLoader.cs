using UnityEngine;

public interface IFileLoader
{

    UserFileData Load( string locator = null );

}

public class LocalFileLoader : IFileLoader
{
    public UserFileData Load( string locator = null )
    {
        string targetLocator = UserFileIO.USERDATA_FILENAME;

        if( locator != null )
            targetLocator = locator;

        string fileContent = UserFileIO.ReadTextUserDataFile( targetLocator );

        if( fileContent == null )
            return null;
        return Json.DeserializeObject<UserFileData>(fileContent);
    }
}
