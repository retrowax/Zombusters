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

Last change: 2010-09-16
*/

using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;


namespace ZombustersWindows
{
    public class TopScoreList
    {
        private int mMaxSize;
        private List<TopScoreEntry> mEntryList;
        private List<TopScoreEntry> mFilteredList;
        private Dictionary<string, TopScoreEntry> mEntryMap;

        private readonly object SYNC = new object();

        public TopScoreList(int maxSize)
        {
            mMaxSize = maxSize;
            mEntryList = new List<TopScoreEntry>();
            mFilteredList = new List<TopScoreEntry>();
            mEntryMap = new Dictionary<string, TopScoreEntry>();
        }

        public TopScoreList(BinaryReader reader)
            : this(reader.ReadInt32())
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                TopScoreEntry entry = new TopScoreEntry(reader, true);
                if (entry.isLegal())
                {
                    mEntryMap[entry.Gamertag] = entry;
                    mEntryList.Add(entry);
                }
            }
        }

        public bool containsEntryForGamertag(string gamertag)
        {
            lock (SYNC)
            {
                for (int i = 0; i < mEntryList.Count; i++)
                {
                    if (mEntryList[i].Gamertag == gamertag)
                        return true;
                }
                return false;
            }
        }

        public int getFullCount()
        {
            lock (SYNC)
            {
                return mEntryList.Count;
            }
        }

        //public int getFilteredCount(SignedInGamer gamer)
        //{
        //    lock (SYNC)
        //    {
        //        initFilteredList(gamer, false);
        //        return mFilteredList.Count;
        //    }
        //}

        public void fillPageFromFullList(int pageNumber, TopScoreEntry[] page)
        {
            lock (SYNC)
            {
                fillPage(mEntryList, true, pageNumber, page);
            }
        }

        //public void fillPageFromFilteredList(int pageNumber, TopScoreEntry[] page, SignedInGamer gamer)
        //{
        //    lock (SYNC)
        //    {
        //        initFilteredList(gamer, true);
        //        fillPage(mFilteredList, false, pageNumber, page);
        //    }
        //}

        public int fillPageThatContainsGamertagFromFullList(TopScoreEntry[] page, string gamertag)
        {
            lock (SYNC)
            {
                int indexOfGamertag = 0;
                for (int i = 0; i < mEntryList.Count; i++)
                {
                    if (mEntryList[i].Gamertag == gamertag)
                    {
                        indexOfGamertag = i;
                        break;
                    }
                }
                int pageNumber = indexOfGamertag / page.Length;
                fillPage(mEntryList, true, pageNumber, page);
                return pageNumber;
            }
        }

        //public int fillPageThatContainsGamertagFromFilteredList(TopScoreEntry[] page, SignedInGamer gamer)
        //{
        //    lock (SYNC)
        //    {
        //        initFilteredList(gamer, true);

        //        int indexOfGamertag = 0;
        //        for (int i = 0; i < mFilteredList.Count; i++)
        //        {
        //            if (mFilteredList[i].Gamertag == gamer.Gamertag)
        //            {
        //                indexOfGamertag = i;
        //                break;
        //            }
        //        }
        //        int pageNumber = indexOfGamertag / page.Length;
        //        fillPage(mFilteredList, false, pageNumber, page);
        //        return pageNumber;
        //    }
        //}

        private void fillPage(List<TopScoreEntry> list, bool initRank, int pageNumber, TopScoreEntry[] page)
        {
            int index = pageNumber * page.Length;
            for (int i = 0; i < page.Length; i++)
            {
                if (index >= 0 && index < list.Count)
                {
                    page[i] = list[index];
                    if (initRank)
                        page[i].RankAtLastPageFill = index + 1;
                }
                else
                    page[i] = null;
                index++;
            }
        }

        //private void initFilteredList(SignedInGamer gamer, bool initRank)
        //{
        //    string gamertag = gamer.Gamertag;
        //    FriendCollection friendsFilter = gamer.GetFriends();
        //    mFilteredList.Clear();
        //    for (int i = 0; i < mEntryList.Count; i++)
        //    {
        //        TopScoreEntry entry = mEntryList[i];
        //        if (entry.Gamertag == gamertag)
        //        {
        //            mFilteredList.Add(entry);
        //            if (initRank)
        //                entry.RankAtLastPageFill = i + 1;
        //        }
        //        else
        //        {
        //            foreach (FriendGamer friend in friendsFilter)
        //            {
        //                if (entry.Gamertag == friend.Gamertag)
        //                {
        //                    mFilteredList.Add(entry);
        //                    if (initRank)
        //                        entry.RankAtLastPageFill = i + 1;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

#if WINDOWS_PHONE
        public void write(BinaryWriter writer)
        {
            writer.Write(mMaxSize);
            writer.Write(mEntryList.Count);
            foreach (TopScoreEntry entry in mEntryList)
                entry.write(writer);
        }
#else
        public void write(BinaryWriter writer)
        {
            lock (SYNC)
            {
                writer.Write(mMaxSize);
                writer.Write(mEntryList.Count);
                foreach (TopScoreEntry entry in mEntryList)
                    entry.write(writer);
            }
        }
#endif

        public bool addEntry(TopScoreEntry entry)
        {
            if (!entry.isLegal())
                return false;

            lock (SYNC)
            {
                string gamertag = entry.Gamertag;
                if (mEntryMap.ContainsKey(gamertag))
                {
                    // Existing entry found for this gamertag
                    TopScoreEntry existingEntry = mEntryMap[gamertag];
                    int compareValue = entry.compareTo(existingEntry);
                    if (compareValue < 0)
                    {
                        // new entry is smaller: do not insert
                        return false;
                    }
                    else if (compareValue == 0)
                    {
                        // both entries are equal: Keep existing entry but transfer "IsLocal" state
                        existingEntry.IsLocalEntry = entry.IsLocalEntry;
                        return false;
                    }
                    else
                    {
                        // existing entry is smaller: replace with new entry
                        mEntryList.Remove(existingEntry);
                        addNewEntry(entry);	// this also replaces existing entry in mEntryMap
                        return true;
                    }
                }
                else
                    return addNewEntry(entry);
            }
        }

        private bool addNewEntry(TopScoreEntry entry)
        {
            for (int i = 0; i < mEntryList.Count; i++)
            {
                if (entry.compareTo(mEntryList[i]) >= 0)
                {
                    // Found existing smaller entry: Insert this one before
                    mEntryList.Insert(i, entry);
                    mEntryMap[entry.Gamertag] = entry;
                    // Delete last entry if there are now too many
                    if (mEntryList.Count > mMaxSize)
                    {
                        TopScoreEntry removedEntry = mEntryList[mMaxSize];
                        mEntryList.RemoveAt(mMaxSize);
                        mEntryMap.Remove(removedEntry.Gamertag);
                    }
                    return true;
                }
            }

            // No existing smaller entry found, but still space in list: Add at end
            if (mEntryList.Count < mMaxSize)
            {
                mEntryList.Add(entry);
                mEntryMap[entry.Gamertag] = entry;
                return true;
            }

            // Entry added at end or No existing smaller entry found and list is full: Do not add
            return false;
        }

        public void initForTransfer()
        {
            lock (SYNC)
            {
                foreach (TopScoreEntry entry in mEntryList)
                    entry.IsLocalEntry = true; // at the beginning of a transfer, all entries are local
            }
        }

#if !WINDOWS_PHONE && !WINDOWS
        public int writeNextTransferEntry(PacketWriter writer, int myListIndex, int entryIndex)
        {
            lock (SYNC)
            {
                while (entryIndex < mEntryList.Count)
                {
                    // While there are still more entries in the current list:
                    // Find a local entry that needs transfer
                    if (mEntryList[entryIndex].IsLocalEntry)
                    {
#if WINDOWS
                        Debug.Write("*"); // local entry transferred
#endif
                        writer.Write(TopScoreListContainer.MARKER_ENTRY);
                        writer.Write((byte)myListIndex);
                        mEntryList[entryIndex].write(writer);
                        return entryIndex + 1;
                    }
                    else
                    {
#if WINDOWS
                        Debug.Write("~"); // remote entry skipped
#endif
                        entryIndex++;
                        Thread.Sleep(1);
                    }
                }
                return -1;
            }
        }

        public bool readTransferEntry(PacketReader reader)
        {
            lock (SYNC)
            {
                return addEntry(new TopScoreEntry(reader, false));
            }
        }
#endif
    }
}
