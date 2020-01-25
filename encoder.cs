/*******************************************************
 *											*
 *		PNG Encoder v1							*
 *		Author: Stephen DeMaria					*
 *	     email: stephendemaria@hotmail.com			*
 *											*
 *******************************************************/

using System;
using System.Drawing;
using ImageFormats;

namespace Encoder
{
     public class Frame
     {

          private int width;
          private int height;
          private int blockSize;
          private double[,] redChannel;
          private double[,] blueChannel;
          private double[,] greenChannel;

          static void Main(string[] args)
          {

               // add path to the input image to be compressed
               string sourceImagePath = @"E:\cs\c#\encoder v1\input.png";

               var frame = new Frame( 16, sourceImagePath);

               frame.importPNGFrame( sourceImagePath, ref frame );

               var compressedMatrix = new Double[frame.width, frame.height];

               frameSegmentation( ref frame, frame.width, frame.height, frame.blockSize );

               // export frame as PNG
               var png = new PNG( frame, frame.width, frame.height, "compressed.png" );

               Console.ReadLine();

          }  // end Main()

          public Frame( int blockSizeInput, string sourceImagePath )
          {

               width = 0; //importPNGFrame() will initialize width and height when image is imported
               height = 0;
               blockSize = blockSizeInput;
               redChannel = new double[width, height];
               greenChannel = new double[width, height];
               blueChannel = new double[width, height];

          }  // end Frame()

          private void importPNGFrame( string filename, ref Frame frame )
          {    // this function creates and returns a matrix of size: [width, height]

               Bitmap image = new Bitmap(filename);
               height = image.Height;
               width = image.Width;

               Console.WriteLine("{0} is an {1}x{2} image.", filename, height, width);

               //var sourceFrame = new double[width, height];
               frame.redChannel = new double[width, height];
               frame.greenChannel = new double[width, height];
               frame.blueChannel = new double[width, height];

               // build new array: sourceImage
               int i = 0;    // counter variables: i and j.
               int j = 0;
               Color pixelColor;

               // timer variables
               int percentTimeLeft;
               int resolution = width * height;

               Console.WriteLine("width = {0}", width);
               Console.WriteLine("height = {0}", height);

               while ( j < height )
               {

                    while ( i < width )
                    {

                         pixelColor = image.GetPixel(i, j);
                         frame.redChannel[i, j] = pixelColor.R;
                         frame.greenChannel[i, j] = pixelColor.G;
                         frame.blueChannel[i, j] = pixelColor.B;

                         i++;

                    }

                    Console.Clear();
                    percentTimeLeft = (int)(((double)(width * (j + 1)) / resolution) * 100);
                    Console.WriteLine("Importing Image: {0}% complete.", percentTimeLeft);

                    i = 0;
                    j++;

               }

          }  // end importPNGFrame()

          public static void displayMatrix(int size, ref double[,] displayThisMatrix )
          {

               int x = 0;
               int y = 0;
               int sizeMinusOne = size - 1;
               int sizeMinusTwo = size - 2;

               while (x <= sizeMinusOne)
               {

                    Console.Write("[ ");

                    while (y <= sizeMinusOne)
                    {

                         Console.Write(displayThisMatrix[x, y]);

                         if (y > sizeMinusTwo)
                         {

                              Console.Write(" ]\n");

                         }
                         else
                         {

                              Console.Write(", ");

                         }

                         y++;

                    }

                    y = 0;
                    x++;

               }

          }  // end displayMatrix()

          public int getFrameWidth()
          {

               return width;

          }

          public int getFrameHeight()
          {

               return height;

          }

          public void setGreenChannelPixel( int x, int y, double pixelValue )
          {

               if ((pixelValue <= 255) & (pixelValue >= 0))
               {

                    greenChannel[x, y] = pixelValue;

               }
               else
               {

                    Console.WriteLine("EXCEPTION: Green pixel ({0},{1}) = {2} and it must be between 0 and 255.", x, y, pixelValue);

                    // temporary fix; some samples are over 255
                    greenChannel[x, y] = pixelValue;

               }

          }  // end setGreenChannelPixel()

          public double getGreenChannelPixel( int x, int y )
          {

               return greenChannel[x, y];

          }

          public void setRedChannelPixel(int x, int y, double pixelValue)
          {

               if ((pixelValue <= 255) & (pixelValue >= 0))
               {

                    redChannel[x, y] = pixelValue;

               }
               else
               {

                    Console.WriteLine( "EXCEPTION: Red pixel ({0},{1}) = {2} and it must be between 0 and 255.", x, y, pixelValue );

                    // temporary fix; some samples are over 255
                    redChannel[x, y] = 255;

               }

          }  // end setRedChannelPixel()

          public double getRedChannelPixel(int x, int y)
          {

               return redChannel[x, y];

          }

          public void setBlueChannelPixel(int x, int y, double pixelValue)
          {

               if ((pixelValue <= 255) & (pixelValue >= 0))
               {

                    blueChannel[x, y] = pixelValue;

               }
               else if( pixelValue > 255 )
               {

                    Console.WriteLine("EXCEPTION: Blue pixel ({0},{1}) = {2} and it must be between 0 and 255.", x, y, pixelValue);

                    // temporary fix; some samples are over 255
                    blueChannel[x, y] = 255;

               }

          }  // end setBlueChannelPixel()

          public double getBlueChannelPixel(int x, int y)
          {

               return blueChannel[x, y];

          }
          private static void frameSegmentation( ref Frame frame, int width, int height, int blockSize )
          {

               int xAxisBlocks = (int)(width / blockSize);
               int yAxisBlocks = (int)(height / blockSize);
               int yBlockCoordinate;
               int xBlockCoordinate;
               int totalFrameBlocks = (int)(xAxisBlocks * yAxisBlocks);

               int percentTimeLeft;

               // Process each block in the frame sequentially
               int blockCounter = 0;

               while ( blockCounter < totalFrameBlocks )
               {

                    // create the x and y coordinates of the block inside the source frame
                    yBlockCoordinate = blockCounter / xAxisBlocks;
                    xBlockCoordinate = blockCounter % xAxisBlocks;

                    // dynamically create a new block
                    //Console.WriteLine("Block {0}", blockCounter); //*
                    var block = new Block(blockSize, xBlockCoordinate, yBlockCoordinate, ref frame );

                    // Display status of image compression
                    Console.Clear();
                    percentTimeLeft = (int)(( (double)blockCounter / (double)totalFrameBlocks ) * 100 ) + 1;
                    Console.WriteLine( "Compressing Image: {0}% complete.", percentTimeLeft );

                    blockCounter++;

               }

          } // end frameSegmentation()

     }  // end class Frame

     public class Block
     {

          const double pi = 3.141592653589f;
          private double[,] redBlockData;
          private double[,] greenBlockData;
          private double[,] blueBlockData;
          int blockSize;

          // For the variables, below, letter[0] denotes an x-coordinate and letter[1] denotes a y.
          private int[] a = new int[2];   // block top-left coordinate in image
          private int[] b = new int[2];   // top-right coordinate
          private int[] c = new int[2];   // bottom-left coordinate
          private int[] d = new int[2];   // bottom-right coordinate

          public Block( int blockSide, int xBlockCoordinate, int yBlockCoordinate, ref Frame sourceFrame)
          {    // blockSize is the length, in pixels, of one side of a square block of pixels.

               blockSize = blockSide;

               // Assign the appropriate coordinates to the block
               a[0] = blockSize * xBlockCoordinate;
               a[1] = blockSize * yBlockCoordinate;
               b[0] = blockSize * (xBlockCoordinate + 1);
               b[1] = blockSize * yBlockCoordinate;
               c[0] = blockSize * xBlockCoordinate;
               c[1] = blockSize * (yBlockCoordinate + 1);
               d[0] = blockSize * (xBlockCoordinate + 1);
               d[1] = blockSize * (yBlockCoordinate + 1);

               redBlockData = new double[blockSize, blockSize];
               greenBlockData = new double[blockSize, blockSize];
               blueBlockData = new double[blockSize, blockSize];

               // create blockData array based on sourceFrame and coordinates: a, b, c, and d.
               int i = a[0];    // i and j are loop counters which denote coordinates within the block,
               int j = a[1];    // where i denotes the horizontal axis.

               // This loop creates a map from global coordinates in (i,j) within the frame
               // to local coordinates within the block (k,l)
               int k = 0;
               int l = 0;

               while ( j < c[1])    // iterates between the y-coordinates of the top-left and bottom left of the block
               {

                    while (i < b[0])
                    {

                         redBlockData[k, l] = sourceFrame.getRedChannelPixel(i, j);
                         greenBlockData[k, l] = sourceFrame.getGreenChannelPixel(i,j); // create mapping
                         blueBlockData[k, l] = sourceFrame.getBlueChannelPixel(i,j);

                         k++;  // increment local counter
                         i++;  // increment global counter

                    }

                    k = 0;      // reset local counter
                    i = a[0];   // reset global counter

                    l++;  // increment local counter
                    j++;  // increment global counter

               }

               //Console.WriteLine("Original Matrix");
               //Frame.displayMatrix( blockSize, ref blueBlockData); //*

               redBlockData = dct( redBlockData );
               greenBlockData = dct( greenBlockData );
               blueBlockData = dct( blueBlockData );

               //Console.WriteLine();
               //Frame.displayMatrix(blockSize, ref blockData); //*

               //Console.WriteLine();

               //Console.WriteLine( "dct() AND Quantize()" );
               quantize();
               //Frame.displayMatrix(blockSize, ref blueBlockData); //*

               //Console.WriteLine();
               //Console.WriteLine("idct()");
               redBlockData = idct( redBlockData );
               greenBlockData = idct( greenBlockData );
               blueBlockData = idct( blueBlockData );

               i = a[0];    // i and j are loop counters which denote coordinates within the block,
               j = a[1];
               k = 0;
               l = 0;

               while (j < c[1])    // iterates between the y-coordinates of the top-left and bottom left of the block
               {

                    while (i < b[0])  // iterates between the x-coodrinates that make up block rows
                    {

                         sourceFrame.setRedChannelPixel( i, j, redBlockData[k, l]);    // create inverse mapping back to original frame
                         sourceFrame.setGreenChannelPixel(i, j, greenBlockData[k, l]);
                         sourceFrame.setBlueChannelPixel(i, j, blueBlockData[k, l]);

                         k++;  // increment local counter
                         i++;  // increment global counter

                    }

                    k = 0;      // reset local counter
                    i = a[0];   // reset global counter

                    l++;  // increment local counter
                    j++;  // increment global counter

               }

          }  // end Block() constructor

          private double[,] dct( double[,] inputMatrix )
          {

               int i = 0;        // i and j are the coordinates to the input matrix: in[][]
               int j = 0;
               int x = 0;        // x and y are the coordinates to the output matrix: out[][]
               int y = 0;
               int sizeTimesTwo = blockSize * 2;    // so this operation is not repeatedly performed in loops
               int sizeMinusOne = blockSize - 1;
               double cosineTermOne;
               double cosineTermTwo;
               double cSubx;
               double cSuby;
               var outputMatrix = new double[blockSize, blockSize];

               while ( x <= sizeMinusOne )
               {

                    while ( y <= sizeMinusOne )
                     {

                         while( i <= sizeMinusOne )
                         {

                             while( j <= sizeMinusOne )
                             {

                                 cosineTermOne = Math.Cos( ( ( 2.0 * i + 1 ) * x * pi ) / sizeTimesTwo );
                                 cosineTermTwo = Math.Cos( ( ( 2.0 * j + 1 ) * y * pi ) / sizeTimesTwo );

                                 outputMatrix[x,y] += inputMatrix[i,j] * cosineTermOne * cosineTermTwo;

                                 j++;

                             }

                             j = 0;
                             i++;

                             if (i > sizeMinusOne)
                             {

                                   if (x == 0)
                                   {

                                        cSubx = Math.Sqrt(1.0 / ((double)blockSize));
                                        outputMatrix[x, y] *= cSubx;

                                   }
                                   else if (x > 0)
                                   {

                                        cSubx = Math.Sqrt(2.0 / ((double)blockSize));
                                        outputMatrix[x, y] *= cSubx;

                                   }

                             }

                             if (i > sizeMinusOne)
                             {


                                   if (y == 0)
                                   {

                                        cSuby = Math.Sqrt(1.0 / ((double)blockSize));
                                        outputMatrix[x,y] *= cSuby;

                                   }
                                   else if (y > 0)
                                   {

                                        cSuby = Math.Sqrt(2.0 / ((double)blockSize));
                                        outputMatrix[x,y] *= cSuby;

                                   }

                             }

                         }

                         i = 0;
                         y++;

                    }

                    y = 0;
                    x++;

               }

               return outputMatrix;

          }  // end dct()

          private void quantize()
          {    // This is a very basic quantizer that removes the last 1/10 of the spatial frequencies
               // in a frame Block which are between firstFrequency and lastFrequency.

               int quantizationCoefficient = 10;   // 10 = no quantization

               int firstFrequency = (int)(((blockSize * blockSize)/10) * quantizationCoefficient );
               int lastFrequency = (blockSize * blockSize) - 1;

               // where does frequency 1 start in the 2d matrix?
               int i = firstFrequency % blockSize;   // i and j are an interval over the x-axis
               int j = lastFrequency % blockSize;
               int k = (int)(firstFrequency / blockSize);   // k and l are an interval over the y-axis
               int l = (int)(lastFrequency / blockSize);

               // discard frequencies
               int yCounter = k;
               int xCounter = i;
               while( yCounter <= l )
               {

                    while(xCounter <= j )
                    {

                         //Console.WriteLine( "({0},{1}) ", xCounter, yCounter );
                         redBlockData[yCounter, xCounter] *= 0;  // remove frequency
                         blueBlockData[yCounter, xCounter] *= 0;
                         greenBlockData[yCounter, xCounter] *= 0.5;  // compress luminance less

                         xCounter++;

                    }

                    xCounter = 0;  // reset xCounter to zero to start at beginning of next line
                    yCounter++;

               }

          }  // end quantize

          private double[,] roundMatrix( double[,] inputMatrix )
          {    // round all matrix elements to the nearest integer.

               int x = 0;
               int y = 0;
               int roundTo = 7;

               while( y < blockSize)
               {

                    while( x < blockSize )
                    {

                         inputMatrix[x, y] = Math.Round( Math.Abs( inputMatrix[x, y] ) );

                         if ( (inputMatrix[x, y] % roundTo) >= 2 )
                         {

                              inputMatrix[x, y] += roundTo - (int)(inputMatrix[x, y] % roundTo);

                              if (inputMatrix[x, y] > 255)
                              {

                                   inputMatrix[x, y] = 255;

                              }

                         }
                         else if ( (inputMatrix[x, y] % 4) < 2 )
                         {

                              inputMatrix[x, y] -= roundTo - (int)(inputMatrix[x, y] % roundTo );

                              if (inputMatrix[x, y] < 0)
                              {

                                   inputMatrix[x, y] = 0;

                              }

                         }

                         x++;

                    }

                    x = 0;
                    y++;

               }

               return inputMatrix;

          }  // end roundMatrix()

          private double[,] idct( double [,] inputMatrix )
          {

               int x = 0;
               int y = 0;
               int i = 0;
               int j = 0;
               double cosineTermOne;
               double cosineTermTwo;
               double csubx;
               double csuby;
               double C = 0;
               double[,] outputMatrix = new double[blockSize,blockSize];
               int sizeTimesTwo = 2 * blockSize;

               while ( i < blockSize )
               {

                    while ( j < blockSize )
                    {

                         while( x < blockSize )
                         {

                              while( y < blockSize )
                              {

                                   cosineTermOne = Math.Cos(((2.0 * i + 1) * x * pi) / sizeTimesTwo);
                                   cosineTermTwo = Math.Cos(((2.0 * j + 1) * y * pi) / sizeTimesTwo);

                                   if (x == 0)
                                   {

                                        csubx = Math.Sqrt(1.0 / ((double)blockSize));
                                        C = inputMatrix[x, y] * csubx;

                                   }
                                   else if (x > 0)
                                   {

                                        csubx = Math.Sqrt(2.0 / ((double)blockSize));
                                        C = inputMatrix[x, y] * csubx;

                                   }

                                   if (y == 0)
                                   {

                                        csuby = Math.Sqrt(1.0 / ((double)blockSize));
                                        C *= csuby;

                                   }
                                   else if (y > 0)
                                   {

                                        csuby = Math.Sqrt(2.0 / ((double)blockSize));
                                        C *= csuby;

                                   }

                                   outputMatrix[i, j] += cosineTermOne * cosineTermTwo * C;

                                   y++;

                              }

                              y = 0;
                              x++;

                         }

                         x = 0;
                         j++;

                    }

                    j = 0;
                    i++;

               }

               return roundMatrix(outputMatrix);

          }

     }  // end class Block

} // end namespace transforms
