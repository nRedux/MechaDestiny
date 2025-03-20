using Newtonsoft.Json;
using UnityEngine;

public interface IFileWriter
{
    void WriteFile( string locator, RunData data );
}

public class LocalFileWriter_v1 : IFileWriter
{
    public void WriteFile( string locator, RunData fileData )
    {
        ActorCollectionEntry e = new ActorCollectionEntry() { Actor = new Actor() };
        var t = Json.SerializeObject( e );
        Debug.Log(t);

        Actor a = new Actor();
        t = Json.SerializeObject( a );
        Debug.Log( t );


        string runDataJson = Json.SerializeObject( fileData );

        var userDataFile = new UserFileData();
        userDataFile.RunDataJson = runDataJson;

        string fileDataJson = Json.SerializeObject( userDataFile );
        UserFileIO.WriteTextUserDataFile( fileDataJson, UserFileIO.USERDATA_FILENAME );
    }
}
