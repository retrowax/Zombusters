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
*/

namespace ZombustersWindows
{
    public interface IOnlineSyncTarget
    {
        /* This method is called by the OnlineDataSyncManager at the very start of the online
         * synchronization between two peers, right after the network session has been established.
         * Use it for any initialization that might be necessary to prepare for the synchronization.
         * 
         * IMPORTANT: The method is called by a separate worker thread. Make sure that it is properly
         * synched with the normal game main thread where necessary (if the two threads may possibly
         * access the same data). */
        void startSynchronization();

        /* This method is called by the OnlineDataSyncManager at the very end of the online
         * synchronization between two peers, right after the network session has been closed.
         * Use it for any cleanup that might be necessary after the synchronization.
         * 
         * IMPORTANT: The method is called by a separate worker thread. Make sure that it is properly
         * synched with the normal game main thread where necessary (if the two threads may possibly
         * access the same data). */
        void endSynchronization(Player player, TopScoreListContainer mScores);

        /* This method is called by the OnlineDataSyncManager during the online synchronization
         * between two peers, right before the local peer starts his own send cycle.
         * Each peer goes through a send cycle and a receive cycle. One of the peers sends first,
         * then receives, the other receives first, then sends.
         * For each of them, this method is called right before the send cycle (so the call might
         * happen after the receive cycle, depending on which of the two peers this is).
         * Use it for any initialization that might be necessary to prepare for sending.
         * 
         * IMPORTANT: The method is called by a separate worker thread. Make sure that it is properly
         * synched with the normal game main thread where necessary (if the two threads may possibly
         * access the same data). */
        void prepareForSending();

        /* This method is called by the OnlineDataSyncManager during the send cycle, to write the next
         * transfer record (and thus send it to the other peer).
         * Return "true" if the written record was the record that signals the end of the send cycle,
         * or "false" if there are more records to transfer. See comment of "readTransferRecord" for
         * more details.
         * 
         * IMPORTANT: The method is called by a separate worker thread. Make sure that it is properly
         * synched with the normal game main thread where necessary (if the two threads may possibly
         * access the same data). */
#if !WINDOWS_PHONE && !WINDOWS
        bool writeTransferRecord(PacketWriter writer);
#endif

        /* This method is called by the OnlineDataSyncManager during the receive cycle, to read the next
         * transfer record (which the other peer has sent us).
         * Return "true" if the received record was recognized as the end-of-cycle record, or "false"
         * if the record was not recognized as the end-of-cycle record (i.e. more records are still
         * expected).
         * 
         * It is up to the implementation of "writeTransferRecord" and "readTransferRecord" to define
         * a protocol that allows the two peers to recognize the end of the transfer cycle. Usually you
         * would transfer some sort of record marker, for example "1" means "data record" (so the "1"
         * is then followed by more data bytes) and "0" means "end of data" (not followed by more data
         * bytes). When the implementation of "writeTransferRecord" writes the end marker "0", it
         * must then return "true". But if it writes the marker "1" (followed by the actual data bytes)
         * it must return "false. And when "readTransferRecord" reads the end marker "0", it must also
         * return "true". But if it reads the marker "1" (and then also reads the following data bytes)
         * it must return "false".
         * 
         * IMPORTANT: The method is called by a separate worker thread. Make sure that it is properly
         * synched with the normal game main thread where necessary (if the two threads may possibly
         * access the same data). */
#if !WINDOWS_PHONE && !WINDOWS
        bool readTransferRecord(PacketReader reader);
#endif
    }
}