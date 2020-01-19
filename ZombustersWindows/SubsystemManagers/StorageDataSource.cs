using System;
using System.IO;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using Bugsnag.Clients;

namespace ZombustersWindows.Subsystem_Managers
{
    public class StorageDataSource
    {
        private readonly BaseClient BugSnagClient;

        public StorageDataSource(ref BaseClient bugSnagClient)
        {
            this.BugSnagClient = bugSnagClient;
        }

        public Object LoadXMLFile(string filename, object returningClass)
        {
            var returningObject = CreateObjectBy(returningClass.GetType()); 

            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(filename, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(returningClass.GetType());
                    returningObject = serializer.Deserialize(isolatedFileStream);
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                BugSnagClient.Notify(exception);
                return returningClass;
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
            return returningObject;
        }

        public void SaveXMLFile(string filename, object classObjectToSave)
        {
            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    XmlSerializer serializer = new XmlSerializer(classObjectToSave.GetType());
                    serializer.Serialize(isolatedFileStream, classObjectToSave);
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                BugSnagClient.Notify(exception);
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
        }

        public TopScoreListContainer LoadScoreList(string filename)
        {
            TopScoreListContainer topScoreListContainer = null;

            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    BinaryReader binaryReader = new BinaryReader(isolatedFileStream);
                    topScoreListContainer = new TopScoreListContainer(binaryReader);
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                BugSnagClient.Notify(exception);
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
            return topScoreListContainer;
        }

        public void SaveScoreListToFile(string filename, TopScoreList[] scoreList)
        {
            BinaryWriter binaryWriter = null;

            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            { 
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    binaryWriter = new BinaryWriter(isolatedFileStream);
                    binaryWriter.Write(scoreList.Length);
                    foreach (TopScoreList list in scoreList)
                        list.write(binaryWriter);
                }
                isolatedFileStream.FlushAsync();
            }
            catch (Exception exception)
            {
                BugSnagClient.Notify(exception);
            }
            finally
            {
                if (isolatedStorageFile != null)
                {
                    isolatedStorageFile.Dispose();
                }
            }
        }

        Object CreateObjectBy(Type clazz)
        {
            Object theObject = Activator.CreateInstance(clazz);
            return theObject;
        }
    }
}