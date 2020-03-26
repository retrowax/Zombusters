/*
Copyright (c) 2010 Spyn Doctor Games (Johannes Hubert). All rights reserved.

Redistribution and use in binary forms, with or without modification, and for whatever
purpose (including commercial) are permitted. Atribution is not required. If you want
to give attribution, use the following text and URL (may be translated where required):
        Uses source code by Spyn Doctor Games - http://www.spyn-doctor.de

Redistribution and use in source forms, with or without modification, are permitted
provided that redistributions of source code retain the above copyright notice, this
list of conditions and the following disclaimer.

THIS SOFTWARE IS PROVIDED BY SPYN DOCTOR GAMES (JOHANNES HUBERT) "AS IS" AND ANY EXPRESS
OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
SPYN DOCTOR GAMES (JOHANNES HUBERT) OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Last change: 2010-12-23
*/

using System.IO;
using System.Diagnostics;


namespace ZombustersWindows
{
    public class TopScoreListContainer : IOnlineSyncTarget
    {
        public TopScoreList[] scoreList;

        private bool isChanged;
        private int transferCurrentListIndex;
        private int transferCurrentEntryIndex;

        public const byte MARKER_ENTRY = 0;					// bit pattern 00000000
        private const byte MARKER_CONTAINER_END = 0x55;		// bit pattern 01010101

        private readonly object SYNC = new object();

        /* User this ctor to create a new empty TopScoreListContainer with "listCount" many top-score
         * lists, where each list can have up to "listMaxSize" many entries. */
        public TopScoreListContainer(int listCount, int listMaxSize)
        {
            scoreList = new TopScoreList[listCount];
            for (int i = 0; i < listCount; i++)
                scoreList[i] = new TopScoreList(listMaxSize);
        }

        /* Use this ctor to load an existing TopScoreListContainer from file. The BinaryReader
         * that reads the file must already have been opened on the file (no data must have been
         * read from the file yet). The ctor will reconstruct all previously saved lists in the
         * container, with all their entries. */
        public TopScoreListContainer(BinaryReader reader)
        {
            int listCount = reader.ReadInt32();
            scoreList = new TopScoreList[listCount];
            for (int i = 0; i < listCount; i++)
                scoreList[i] = new TopScoreList(reader);
        }

        /* Checks if the list with the given index contains an entry for the given gamertag. */
        public bool containsEntryForGamertag(int listIndex, string gamertag)
        {
            return scoreList[listIndex].containsEntryForGamertag(gamertag);
        }

        /* Fills the "page" array with one page from the full top score list with the given index.
         * "pageNumber" specifies the number of the page that is requested (0-based).
         * The page size is inferred from the length of the "page" array.
         * Example: If page.Length == 10, and pageNumber is 0, then the array will be filled with the
         * entries 1-10 from the list. If pageNumber is 4, then the array will be filled with the
         * entries 41-50. If not enough entries are available, then the remaining fields of the
         * array are filled with "null".
         * After this method returns, each entries actual rank in the *full* score list can be retrieved
         * from the RankAtLastPageFill property of the entry. */
        public void fillPageFromFullList(int listIndex, int pageNumber, TopScoreEntry[] page)
        {
            scoreList[listIndex].fillPageFromFullList(pageNumber, page);
        }

        /* Fills the "page" array with one page from the filtered top score list with the given index.
         * The filtered list contains only the entry for the given "gamer" and his friends
         * and is therefore normally shorter than the full top score list with the same index.
         * "pageNumber" specifies the number of the page that is requested (0-based).
         * The page size is inferred from the length of the "page" array.
         * Example: If page.Length == 10, and pageNumber is 0, then the array will be filled with the
         * entries 1-10 from the list. If pageNumber is 4, then the array will be filled with the
         * entries 41-50. If not enough entries are available, then the remaining fields of the
         * array are filled with "null".
         * After this method returns, each entries actual rank in the *full* score list can be retrieved
         * from the RankAtLastPageFill property of the entry. */
        //public void fillPageFromFilteredList(int listIndex, int pageNumber, TopScoreEntry[] page, SignedInGamer gamer)
        //{
        //    mScoreLists[listIndex].fillPageFromFilteredList(pageNumber, page, gamer);
        //}

        /* Fills the "page" array with a specific page from the full top score list with the given index.
         * The requested page is the one that contains the specified gamertag. If the gamertag is not
         * present in the full list, then the array is filled with the first page of the full list.
         * The page size is inferred from the length of the "page" array.
         * Example: If page.Length == 10, and the specified gamertag is currently at position 37 in the
         * full list, then the array will be filled with the entries 31-40. If not enough entries are 
         * available, then the remaining fields of the array are filled with "null".
         * After this method returns, each entries actual rank in the *full* score list can be retrieved
         * from the RankAtLastPageFill property of the entry. */
        public int fillPageThatContainsGamertagFromFullList(int listIndex, TopScoreEntry[] page, string gamertag)
        {
            return scoreList[listIndex].fillPageThatContainsGamertagFromFullList(page, gamertag);
        }

        /* Fills the "page" array with a specific page from the filtered top score list with the given index.
         * The filtered list contains only the entry for the given "gamer" and his friends
         * and is therefore normally shorter than the full top score list with the same index.
         * The requested page is the one that contains the specified gamertag. If the gamertag is not
         * present in the filtered list, then the array is filled with the first page of the filtered list.
         * The page size is inferred from the length of the "page" array.
         * Example: If page.Length == 10, and the specified gamertag is currently at position 37 in the
         * filtered list, then the array will be filled with the entries 31-40. If not enough entries are 
         * available, then the remaining fields of the array are filled with "null".
         * After this method returns, each entries actual rank in the *full* score list can be retrieved
         * from the RankAtLastPageFill property of the entry. */
        //public int fillPageThatContainsGamertagFromFilteredList(int listIndex, TopScoreEntry[] page, SignedInGamer gamer)
        //{
        //    return mScoreLists[listIndex].fillPageThatContainsGamertagFromFilteredList(page, gamer);
        //}

#if WINDOWS_PHONE || WINDOWS || NETCOREAPP
        /* Saves the TopScoreListContainer, and all lists and entries in it, into the given BinaryWriter.
         * Usually you would first open a FileStream on which you then construct the BinaryWriter, so
         * that the TopScoreListContainer is written to a file. The resulting file can then be used
         * to later reconstruct the TopScoreListContainer by using the ctor that takes a BinaryReader
         * as an argument. */
        public void save(BinaryWriter writer)
        {
            // Lock to make sure there are no changes while saving
            writer.Write(scoreList.Length);
            foreach (TopScoreList list in scoreList)
                list.write(writer);
        }

        /* Adds a new entry to the list with the specified index and notifies the
         * OnlineDataSyncManager about it. */
        public void addEntry(int listIndex, TopScoreEntry entry)
        {
            // Lock to sync the change with a possible background saving
            lock (SYNC)
            {
                scoreList[listIndex].addEntry(entry);
            }
        }
#else
        public void save(BinaryWriter writer)
        {
            // Lock to make sure there are no changes while saving
            lock (SYNC) {
                writer.Write(mScoreLists.Length);
                foreach (TopScoreList list in mScoreLists)
                    list.write(writer);
            }
        }

        public void addEntry(int listIndex, TopScoreEntry entry, OnlineDataSyncManager manager)
        {
            // Lock to sync the change with a possible background saving
            lock (SYNC) {
                if (mScoreLists[listIndex].addEntry(entry) && manager != null)
                    manager.notifyAboutNewLocalEntry();
            }
        }
#endif

        /* Returns the count of entries in the full list with the specified index. */
        public int getFullListSize(int listIndex)
        {
            return scoreList[listIndex].getFullCount();
        }

        /* Returns the count of entries in the filtered list with the specified index.
         * The filtered list contains only the entry for the given "gamer" and his friends
         * and is therefore normally shorter than the full top score list with the same index. */
        //public int getFilteredListSize(int listIndex, SignedInGamer gamer)
        //{
        //    return mScoreLists[listIndex].getFilteredCount(gamer);
        //}

        public void startSynchronization()
        {
            isChanged = false;
            // Lock to sync the change with a possible background saving
            lock (SYNC)
            {
                foreach (TopScoreList list in scoreList)
                    list.initForTransfer();
            }
        }

        public void endSynchronization(Player player, TopScoreListContainer mScores)
        {
#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
            if (mChanged) {
                // TODO: Trigger saving of container here
                if (player.Device != null)
                {
                    string filename = "leaderboard.txt";
                    Stream stream = player.Container.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    BinaryWriter writer = new BinaryWriter(stream);
                    mScores.save(writer);
                    writer.Close();
                    stream.Close();
                }
                mChanged = false;
            }
#endif
        }

        public void prepareForSending()
        {
            transferCurrentListIndex = 0;
            transferCurrentEntryIndex = 0;
        }

#if !WINDOWS_PHONE && !WINDOWS && !NETCOREAPP
        public bool readTransferRecord(PacketReader reader)
        {
            byte marker = reader.ReadByte();
            switch (marker)
            {
                case MARKER_ENTRY:
                    int listIndex = reader.ReadByte();
                    if (listIndex >= 0 && listIndex < mScoreLists.Length)
                    {
                        // Lock to sync the change with a possible background saving
                        lock (SYNC)
                        {
                            mChanged |= mScoreLists[listIndex].readTransferEntry(reader);
                        }
#if WINDOWS
                        Debug.Write("o"); // entry received
#endif
                    }
#if WINDOWS
                    else
                        Debug.Write("."); // entry with illegal list-index or transfer-seq-no skipped
#endif
                    break;
                case MARKER_CONTAINER_END:
                    Debug.WriteLine("\nEnd marker received");
                    return true;
            }
            return false;
        }

        public bool writeTransferRecord(PacketWriter writer)
        {
            while (true)
            {
                TopScoreList list = mScoreLists[mTransferCurrentListIndex];

                mTransferCurrentEntryIndex = list.writeNextTransferEntry(writer, mTransferCurrentListIndex, mTransferCurrentEntryIndex);
                if (mTransferCurrentEntryIndex > -1)
                    return false;

                // Current list complete:
                Debug.WriteLine("\nList " + mTransferCurrentListIndex + " complete");
                mTransferCurrentListIndex++;
                if (mTransferCurrentListIndex < mScoreLists.Length)
                {
                    // Still more lists to go: Reset list index
                    Debug.WriteLine("Starting with list " + mTransferCurrentListIndex);
                    mTransferCurrentEntryIndex = 0;
                }
                else
                {
                    // No more lists to go: write end for container
                    Debug.WriteLine("All lists complete");
                    writer.Write(MARKER_CONTAINER_END);
                    return true;
                }
            }
        }
#endif
    }
}
