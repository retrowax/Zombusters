using System;
using System.IO;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using Bugsnag;
using System.Reflection;

namespace ZombustersWindows.Subsystem_Managers
{
    public class StorageDataSource
    {
        private readonly Client BugSnagClient;

        public StorageDataSource(ref Client bugSnagClient)
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
#if WINDOWS
                isolatedFileStream.FlushAsync();
#endif
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

                if (isolatedStorageFile.FileExists(filename)) {
                    isolatedStorageFile.DeleteFile(filename);
                }

                using (isolatedFileStream = isolatedStorageFile.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    XmlSerializer serializer = new XmlSerializer(classObjectToSave.GetType());
                    serializer.Serialize(isolatedFileStream, classObjectToSave);
                }
#if WINDOWS
                isolatedFileStream.FlushAsync();
#endif
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
#if WINDOWS
                isolatedFileStream.FlushAsync();
#endif
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
#if WINDOWS
                isolatedFileStream.FlushAsync();
#endif
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