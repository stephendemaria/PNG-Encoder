namespace CyclicRedundancyCheck
{
     class CRC
     {

          /* Table of CRCs of all 8-bit messages. */
          long[] crc_table = new long[256];

          /* Flag: has the table been computed? Initially false. */
          int crc_table_computed = 0;

          /* Make the table for a fast CRC. */
          void make_crc_table()
          {
               long c;
               int n, k;

               for (n = 0; n < 256; n++)
               {

                    c = (long)n;

                    for (k = 0; k < 8; k++)
                    {

                         if ((c & 1) == 1)
                         {

                              c = 0xedb88320L ^ (c >> 1);

                         }
                         else
                         {

                              c = c >> 1;

                         }

                    }

                    crc_table[n] = c;
               }

               crc_table_computed = 1;

          }   // end make_crc_table()

          /* Update a running CRC with the bytes buf[0..len-1]--the CRC
		   should be initialized to all 1's, and the transmitted value
		   is the 1's complement of the final running CRC (see the
		   crc() routine below)). */

          long update_crc(long crc, byte[] buf, int len)
          {

               long c = crc;
               int n;

               if (crc_table_computed == 0)
               {

                    make_crc_table();

               }

               for (n = 0; n < len; n++)
               {
                    c = crc_table[(c ^ buf[n]) & 0xff] ^ (c >> 8);
               }

               return c;
          }

          /* Return the CRC of the bytes buf[0..len-1]. */
          public long crc(byte[] buf, int len)
          {

               return update_crc(0xffffffffL, buf, len) ^ 0xffffffffL;

          }


     }

}