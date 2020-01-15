using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Globalization;
using ZombustersWindows;
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

        public BinaryReader GetBinaryReader(string filename)
        {
            BinaryReader binaryReader = null;

            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    binaryReader = new BinaryReader(isolatedFileStream);
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
            return binaryReader;
        }

        public BinaryWriter GetBinaryWriter(string filename)
        {
            BinaryWriter binaryWriter = null;

            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForDomain();
            try
            { 
                IsolatedStorageFileStream isolatedFileStream = null;
                using (isolatedFileStream = isolatedStorageFile.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    binaryWriter = new BinaryWriter(isolatedFileStream);
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
            return binaryWriter;
        }

        Object CreateObjectBy(Type clazz)
        {
            Object theObject = Activator.CreateInstance(clazz);
            return theObject;
        }
    }
}