using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;   // for DeflateStream class
using Encoder;
using CyclicRedundancyCheck;

namespace ImageFormats
{
	class PNG
	{

		private byte[] bitDepth;
		private int width;
		private int height;

		public PNG( Frame inputFrame, int frameWidth, int frameHeight, string filename )
		{

			width = frameWidth;
			height = frameHeight;

			byte[] ihdrChunk = writeIHDRChunk( width, height, 8, 2 );
			byte[] pHYsChunk = writepHYsChunk( 2835 );
			byte[] idatChunk = writeIDATChunk( inputFrame );

			var directory = Directory.GetCurrentDirectory();
			var file = Path.Combine(directory, filename);

			try
			{

				var filestream = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
				filestream.Write( ihdrChunk, 0, ihdrChunk.Length );
				filestream.Write( pHYsChunk, 0, pHYsChunk.Length );
				filestream.Write( idatChunk, 0, idatChunk.Length );
				filestream.Close();

				Console.WriteLine( "{0} was successfully exported.", filename );

			}
			catch (IOException e)
			{

				Console.WriteLine( e );
				Console.WriteLine("EXPORT ERROR: file was not created.");

			}

		}  // end PNG()

		public byte[] writeIHDRChunk( int width, int height, byte bitdepth, byte colortype )
		{    // this function writes the IDAT chunk and file signature of the PNG file
			// and is called first when writing a PNG file

			// pre-defined byte sequences
			byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
			byte[] ihdrChunk = { 73, 72, 68, 82 };   //73 72 68 82 = IHDR
			byte[] compressionMethod = { 0 };
			byte[] filterMethod = { 0 };
			byte[] interlaceMethod = { 0 };

			// computed byte sequences
			byte[] ihdrLength;      // number of bytes in IHDR chunk

			byte[] imageWidth = new byte[ width ];
			imageWidth = decimalToByteSequence( width, 4, true );

			byte[] imageHeight = new byte[ height ];
			imageHeight = decimalToByteSequence( height, 4, true );

			bitDepth = new byte[1];
			setBitDepth( bitdepth );

			byte[] colorType = new byte[ 1 ];
			colorType[0] = colortype;

			// compile ihdr data
			var ihdrData = new List<byte>();
			ihdrData.AddRange(imageWidth);
			ihdrData.AddRange(imageHeight);
			ihdrData.AddRange(bitDepth);
			ihdrData.AddRange(colorType);
			ihdrData.AddRange(compressionMethod);
			ihdrData.AddRange(filterMethod);
			ihdrData.AddRange(interlaceMethod);
			byte[] ihdrDataChunk = ihdrData.ToArray();

			// get length of ihdr data
			ihdrLength = decimalToByteSequence( ihdrDataChunk.Length, 4, true);

			// add ihdr chunk type code
			var ihdrAddTypeCode = new List<byte>();
			ihdrAddTypeCode.AddRange( ihdrChunk );
			ihdrAddTypeCode.AddRange( ihdrDataChunk );
			byte[] ihdrDataAndTypeCode = ihdrAddTypeCode.ToArray();

			// perform cyclic redundancy check on ihdr
			var crc = new CRC();
			long checkSumDecimal = crc.crc( ihdrDataAndTypeCode, ihdrDataAndTypeCode.Length );
			byte[] checkSum = decimalToByteSequence( checkSumDecimal, 4, true);

			// combine png file signature, ihdrlength, data, type code, and checksum
			var ihdrComplete = new List<byte>();
			ihdrComplete.AddRange(signature);
			ihdrComplete.AddRange(ihdrLength);
			ihdrComplete.AddRange(ihdrDataAndTypeCode);
			ihdrComplete.AddRange(checkSum);
			byte[] outputIHDR = ihdrComplete.ToArray();

			return outputIHDR;

		}  // end writeIHDRChunk()

		public byte[] writeIDATChunk( Frame inputFrame )
		{

			byte[] idatchunkTypeCode = { 73, 68, 65, 84 };  // IDAT
			byte[] iendchunkTypeCode = { 73, 69, 78, 68 };  // IEND
			byte[] iendLength = { 0, 0, 0, 0 };
			byte[] idatChecksum = new byte[4];
			byte[] iendChecksum = new byte[4];
			long decimalCheckSum;

			// compile ihdr data and get length
			byte[] idatData = DEFLATE(inputFrame);
			byte[] idatLength = decimalToByteSequence( idatData.Length, 4, true );

			// add idat chunk type code
			var idatAddChunkType = new List<byte>();
			idatAddChunkType.AddRange( idatchunkTypeCode );
			idatAddChunkType.AddRange( idatData );
			byte[] idatDataPlusChunkType = idatAddChunkType.ToArray();

			// Compute CRC checksum on idatDataPlusChunkType
			var crc = new CRC();
			decimalCheckSum = crc.crc( idatDataPlusChunkType, idatDataPlusChunkType.Length);
			idatChecksum = decimalToByteSequence( decimalCheckSum, 4, true );

			// Compute CRC checksum on iendchunk
			decimalCheckSum = crc.crc(iendchunkTypeCode, iendchunkTypeCode.Length);
			iendChecksum = decimalToByteSequence( decimalCheckSum, 4, true );

			// combine all elements of idat chunk and add iend chunk
			var idatComplete = new List<byte>();
			idatComplete.AddRange( idatLength );
			idatComplete.AddRange( idatDataPlusChunkType );
			idatComplete.AddRange( idatChecksum );

			idatComplete.AddRange( iendLength );
			idatComplete.AddRange( iendchunkTypeCode );
			idatComplete.AddRange( iendChecksum );
			byte[] idatChunk = idatComplete.ToArray();

			return idatChunk;

		}  // end writeIDATChunk()

		public byte[] decimalToByteSequence( long i, int maxBytes, bool endianness )
		{    // this function solves the equation i = (a * 256^maxBytes) + (b * 256^(maxBytes - 1)) + ... + (x * 256^2) + (y * 256^1) + (z * 256^0)

			byte[] outputByteSequence = new byte[ maxBytes ];
			int byteNumber = maxBytes - 1;
			int counter = 0;

			//Console.WriteLine( "i = {0}", i );

			// check if variable, i, can be represented with the number of bytes, maxBytes, specified by the user.
			int bytesNeeded = (int)Math.Ceiling( Math.Log( i, 256 ) );

			// add functionality to change the endianness of the output byte sequence

			if (bytesNeeded <= maxBytes)
			{

				// build outputByteSequence
				while (counter < maxBytes)
				{

					if (Math.Pow(256, byteNumber) > i)
					{

						outputByteSequence[counter] = 0;

					}
					else if (Math.Pow(256, byteNumber) <= i)
					{

						outputByteSequence[counter] = (byte)(i / Math.Pow(256, byteNumber));
						i -= (long)(Math.Pow(256, byteNumber) * outputByteSequence[counter]);

					}

					byteNumber--;
					counter++;

				}

			}
			else
			{

				Console.WriteLine( "EXCEPTION: {0} can not be represented with {1} bytes.", i, maxBytes );

			}

			return outputByteSequence;

		}  // end decimalToByteSequence()

		public void setBitDepth( byte bitDepthValue )
		{

			if (bitDepthValue == 8)
			{

				bitDepth[0] = 8;

			}
			else if (bitDepthValue == 16)
			{

				bitDepth[0] = 16;

			}
			else
			{

				Console.Write( "EXCEPTION: invalid value for PNG bit depth." );
				// throw exception

			}

		}  // end setBitDepth()

		public byte[] DEFLATE( Frame inputFrame )
		{

			int yCounter = 0;
			int xCounter = 0;
			int masterCounter = 0;
			int width = inputFrame.getFrameWidth();
			int height = inputFrame.getFrameHeight();

			// pre-defined and computed byte sequences
			byte[] checkSum = { 0, 0, 0, 0 };
			byte[] methodCode = { 0x78 };   // 0x78 = 120 sub 10 = 01111000 sub 2
			byte[] windowSize = { 0xda };   // leftmost three bits of window size
			byte[] filterTypeByte = { 0 };

			byte[] toAdlerChecksum = { methodCode[0], windowSize[0] };

			// convert 3, 2d arrays to 1, 1d array
			int arraySize = ( 3 * width * height ) + height;  // calculates arraySize for three values per pixel and adds height at the end
			                                                     // to include the filterTypeByte at the beginning of each line.
			var idatData = new byte[ arraySize ];

			while (yCounter < height)
			{

				idatData[masterCounter] = filterTypeByte[0];
				masterCounter++;

				while (xCounter < width )
				{

					idatData[masterCounter] = (byte)inputFrame.getRedChannelPixel(xCounter, yCounter);
					masterCounter++;

					idatData[masterCounter] = (byte)inputFrame.getGreenChannelPixel(xCounter, yCounter);
					masterCounter++;

					idatData[masterCounter] = (byte)inputFrame.getBlueChannelPixel(xCounter, yCounter);
					masterCounter++;

					xCounter++;

				}

				xCounter = 0;
				yCounter++;

			}

			// run DEFLATE
			using ( MemoryStream outputStream = new MemoryStream() )
			using( DeflateStream dfStream = new DeflateStream( outputStream, CompressionLevel.Optimal ))  // CompressionMode.Compress
			{

				dfStream.Write( idatData, 0, idatData.Length );
				dfStream.Close();

				byte[] data = outputStream.ToArray();


				var idatList = new List<byte>();
				idatList.AddRange( methodCode );  // "For PNG compression method 0, the zlib compression method/flags code must specify method code 8 ("deflate" compression)" http://libpng.org/pub/png/spec/1.2/PNG-Compression.html
				idatList.AddRange( windowSize );
				idatList.AddRange( data );   // idatData
				idatList.AddRange( checkSum );
				byte[] outputByteSequence = idatList.ToArray();

				return outputByteSequence;

			}

		}  // end DEFLATE()

		public byte[] writepHYsChunk( int ppm )
		{

			byte[] chunkType = { 112, 72, 89, 115 };
			byte[] length;
			byte[] pixelsPerUnitWidthX = decimalToByteSequence( ppm, 4, true );
			byte[] pixelsPerUnitHeightY = decimalToByteSequence( ppm, 4, true );
			byte[] unitSpecifier = { 0 };
			byte[] pHYsChecksum;
			long decimalCheckSum;

			// build data chunk
			var pHYsList = new List<byte>();
			pHYsList.AddRange( pixelsPerUnitWidthX );
			pHYsList.AddRange( pixelsPerUnitHeightY );
			pHYsList.AddRange( unitSpecifier );
			byte[] pHYsListDataChunk = pHYsList.ToArray();

			// get length of data chunk
			length = decimalToByteSequence(pHYsListDataChunk.Length, 4, true );

			// add chunkType and data chunk
			var pHYsTypeAndDataList = new List<byte>();
			pHYsTypeAndDataList.AddRange( chunkType );
			pHYsTypeAndDataList.AddRange( pHYsListDataChunk );
			byte[] pHYsChunkTypeAndDataChunk = pHYsTypeAndDataList.ToArray();

			// perform cyclic redundancy check
			var crc = new CRC();
			decimalCheckSum = crc.crc(pHYsChunkTypeAndDataChunk, pHYsChunkTypeAndDataChunk.Length);
			pHYsChecksum = decimalToByteSequence(decimalCheckSum, 4, true);

			// assemble final pHYs chunk and outputByteSequence
			var pHYsChunkList = new List<byte>();
			pHYsChunkList.AddRange( length );
			pHYsChunkList.AddRange( pHYsChunkTypeAndDataChunk );
			pHYsChunkList.AddRange( pHYsChecksum );
			byte[] pHYsChunk = pHYsChunkList.ToArray();

			return pHYsChunk;

		}  // end writepHYsChunk()

	}  // end class PNG

}  // end namespace ImageFormats
