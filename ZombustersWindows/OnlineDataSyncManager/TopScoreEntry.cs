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

using System.IO;

namespace ZombustersWindows
{
	public class TopScoreEntry
	{
		private string mGamertag;
		public string Gamertag
		{
			get { return mGamertag; }
		}

		private int mScore;
		public int Score
		{
			get { return mScore; }
		}

		// Local (true) means: This score was in the list before the latest score transfer.
		// Non-local (false) means: This score was freshly transferred into the list from another Xbox.
		private bool mIsLocalEntry;
		public bool IsLocalEntry
		{
			get { return mIsLocalEntry; }
			set { mIsLocalEntry = value; }
		}

		private int mRankAtLastPageFill;
		public int RankAtLastPageFill
		{
			get { return mRankAtLastPageFill; }
			set { mRankAtLastPageFill = value; }
		}

		public TopScoreEntry(string gamertag, int score)
		{
			// freshly created local entry
			mGamertag = gamertag;
			mScore = score;
			mIsLocalEntry = true;
		}


		public TopScoreEntry(BinaryReader reader, bool isLocalEntry)
		{
			// isLocalEntry == true: local entry read from storage
			// isLocalEntry == false: entry from remote source read from online transfer
			mGamertag = reader.ReadString();
			mScore = reader.ReadInt32();
			mIsLocalEntry = isLocalEntry;
		}

		public int compareTo(TopScoreEntry other)
		{
			if (mScore < other.mScore)		// lower score is lower in rank
				return -1;
			else if (mScore > other.mScore) // higher score is higher in rank
				return 1;
			else							// same score, same rank
				return 0;
		}

		public void write(BinaryWriter writer)
		{
			writer.Write(mGamertag);
			writer.Write(mScore);
			// mRankAtLastPageFill is not saved by design!
		}

		public bool isLegal()
		{
			if (mGamertag == null || mGamertag.Length < 1 || mGamertag.Length > 15 || mScore < 0)
				return false;
			for (int i = 0; i < mGamertag.Length; i++) {
				char c = mGamertag[i];
				if (!(c == ' ' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9'))
					return false;
			}
			return true;
		}
	}
}
