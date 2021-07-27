using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red ,green, blue;
    }
    public struct HeapNode
    {
        public double data;
        public int index;
    }

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


       /// <summary>
       /// Apply Gaussian smoothing filter to enhance the edge detection 
       /// </summary>
       /// <param name="ImageMatrix">Colored image matrix</param>
       /// <param name="filterSize">Gaussian mask size</param>
       /// <param name="sigma">Gaussian sigma</param>
       /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

           
            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }


    }

    public class MinHeap
    {
        public HeapNode[] _elements;  //0based arr
        private int _size;

        public MinHeap(int size)
        {
            _elements = new HeapNode[size];
        }

        //next FUNCs is O(1)
        private int GetLeftChildIndex(int ParentIndex) { return 2 * ParentIndex + 1; }
        private int GetRightChildIndex(int ParentIndex) { return 2 * ParentIndex + 2; }
        private int GetParentIndex(int ChildIndex) { return (ChildIndex - 1) / 2; }

        private bool HasLeftChild(int elementIndex) { return GetLeftChildIndex(elementIndex) < _size; }
        private bool HasRightChild(int elementIndex) { return GetRightChildIndex(elementIndex) < _size; }
        private bool IsRoot(int elementIndex) { return elementIndex == 0; }

        private HeapNode GetLeftChild(int elementIndex) { return _elements[GetLeftChildIndex(elementIndex)]; }
        private HeapNode GetRightChild(int elementIndex) { return _elements[GetRightChildIndex(elementIndex)]; }
        private HeapNode GetParent(int elementIndex) { return _elements[GetParentIndex(elementIndex)]; }

        private void Swap(int firstIndex, int secondIndex)     //O(1)
        {
            HeapNode temp = _elements[firstIndex];            //O(1)
            _elements[firstIndex] = _elements[secondIndex];   //O(1)
            _elements[secondIndex] = temp;                    //O(1)
        }

        public void display()                                 //O(n)
        {
            for (int i = 0; i < _size; i++)
                Console.WriteLine(_elements[i].index + "\t" + _elements[i].data);
        }

        public bool IsEmpty()                     //O(1) 
        {
            return _size == 0;                    //O(1)
        }

        public HeapNode Pop()                    //O(Log(n))
        {
            if (_size == 0)
                throw new IndexOutOfRangeException();      //O(1)

            HeapNode result = _elements[0];              //O(1)
            _elements[0] = _elements[_size - 1];        //O(1)
            _size--;                                   //O(1) 

            ReCalculateDown(0);                       //O(Log(n))

            return result;                        
        }

        public void Add(HeapNode element)       //O(Log(n))
        {
            if (_size == _elements.Length)            //O(1)
                throw new IndexOutOfRangeException();

            _elements[_size] = element;             //O(1)
            _size++;                               //O(1)

            ReCalculateUp(_size);                 //O(Log(n))   
        }

        /// <summary>
        /// the error is in this functions////
        /// need to make another func make update to any node
        /// change it if you can
        /// </summary>
        /// <param name="element"></param>
        public void update(HeapNode element)   //O(n)
        { 

            //////read the comment aboveeeeeeee
            if (_size == 0)                               //O(1)
                throw new IndexOutOfRangeException();

            HeapNode oldelement;
            for (int i = 0; i < _size; i++)                //O(n)
                if (_elements[i].index == element.index)   //O(1) 
                {
                    oldelement = _elements[i];           //O(1)
                    _elements[i].data = element.data;   //O(1) 
                    ReCalculateUp(i + 1);              //O(Log(n)) 
                    break;
                }

            //////////////////////testing////////
            /*
            if (HasLeftChild(i) && _elements[i].data > _elements[GetLeftChildIndex(i)].data)
                ReCalculateDown(i);
            else if (HasRightChild(i) && _elements[i].data > _elements[GetRightChildIndex(i)].data)
                ReCalculateDown(i);
            else if (i!= 0) 
            {
                if (HasLeftChild(i) && _elements[i].data < _elements[GetLeftChildIndex(i)].data)
                    ReCalculateUp(GetLeftChildIndex(i) + 1);
                else if (HasRightChild(i) && _elements[i].data < _elements[GetRightChildIndex(i)].data)
                    ReCalculateUp(GetRightChildIndex(i) + 1);
            }
            ///////////testing /////////////////////*/
            
        }

        private void ReCalculateDown(int index)   //O(Log(n))
        {
            while (HasLeftChild(index))          
            {
                int smallerIndex = GetLeftChildIndex(index);
                if (HasRightChild(index) && GetRightChild(index).data < GetLeftChild(index).data)
                {
                    smallerIndex = GetRightChildIndex(index);
                }

                if (_elements[smallerIndex].data >= _elements[index].data)
                {
                    break;
                }

                Swap(smallerIndex, index);    //O(1)
                index = smallerIndex;         //O(1)
            }
        }

        private void ReCalculateUp(int index)     //O(Log(n))
        {
            index--; ;
            while (!IsRoot(index) && _elements[index].data < GetParent(index).data) //O(Log(n))
            {
                int parentIndex = GetParentIndex(index);
                Swap(parentIndex, index);      //O(1)
                index = parentIndex;
            }
        }

        public void simplerun()
        {
            MinHeap mh = new MinHeap(100);

            HeapNode hp;
            hp.data = 10; hp.index = 0;
            mh.Add(hp);

            hp.data = 5; hp.index = 1;
            mh.Add(hp);

            hp.data = 3; hp.index = 2;
            mh.Add(hp);

            hp.data = 12; hp.index = 3;
            mh.Add(hp);

            hp.data = 5; hp.index = 4;
            mh.Add(hp);

            hp.data = 3; hp.index = 5;
            mh.Add(hp);

            mh.display();

            Console.WriteLine();
            Console.WriteLine("\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\");

            hp.index = 2;
            hp.data = 4;
            mh.update(hp);

            mh.display();

            Console.WriteLine();
            Console.WriteLine("\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\");

            HeapNode x = mh.Pop();
            Console.WriteLine(x.index + "\t" + x.data);
            x = mh.Pop();
            Console.WriteLine(x.index + "\t" + x.data);
            x = mh.Pop();
            Console.WriteLine(x.index + "\t" + x.data);
            x = mh.Pop();
            Console.WriteLine(x.index + "\t" + x.data);

            Console.ReadKey();


        }
    }

    public class PROCESSING_DATA
    {
        public static int count_dis ;
        public static RGBPixel[] distinct_colors;
        //public static int[,] colors_indexs;
        public static IDictionary<RGBPixel, int> dic;
        public static double[,] FullyConnectedGraph;
        public static double MST_Sum =0;
        public static double[] distance;
        public static int[] parent ;
        public static int Kclusters;
        public static int []clustersNo;
        public static RGBPixelD[] clustersData;
        public static int[] clusters_count;
     
        public static void Get_Distinict_Colors(RGBPixel[,] Imagedata, int height, int width) //O(n^2)
        {
            //HashSet<RGBPixel> hashset = new HashSet<RGBPixel>();
            dic = new Dictionary<RGBPixel, int>(height*width);
            //colors_indexs = new int [height,width];
            distinct_colors = new RGBPixel[width*height];
            PROCESSING_DATA.count_dis = 0;
           
                for (int i = 0; i < height; i++)        //O(n) *body
                    for (int j = 0; j < width; j++)    //O(n)  *body
                    {
                        if (!dic.ContainsKey(Imagedata[i,j]))  //O(1)
                        {
                          //  hashset.Add(Imagedata[i, j]);       //O(1)
                            distinct_colors[PROCESSING_DATA.count_dis] = Imagedata[i, j]; //O(1)
                            dic.Add(Imagedata[i,j],count_dis);                           //O(1)
                            PROCESSING_DATA.count_dis++;                                //O(1)
                        }
                    // colors_indexs[i,j] = dic[Imagedata[i,j]];  //O(1)
                    }
        }
        public static void Make_Fully_Connected_Graph()  //O(d^2)
        {
            FullyConnectedGraph = new double[count_dis,count_dis];
            double result;
            for (int i = 0; i < count_dis; i++) //O(d) * body order
            {
                for (int j = 0; j < count_dis; j++) //O(d) * body order
                {
                    result = (distinct_colors[i].red - distinct_colors[j].red) * (distinct_colors[i].red - distinct_colors[j].red)
                           + (distinct_colors[i].green - distinct_colors[j].green) * (distinct_colors[i].green - distinct_colors[j].green)
                           + (distinct_colors[i].blue - distinct_colors[j].blue) * (distinct_colors[i].blue - distinct_colors[j].blue);//o(1)
                    FullyConnectedGraph[i, j] = Math.Sqrt(result); //O(1)
                }
            }
        }
        public static void Make_Mst_sum() //O(d^2)
        {
            distance = new double[count_dis];
            parent = new int[count_dis];
            bool[] visited = new bool[count_dis];
            MinHeap minheap = new MinHeap(count_dis);
            HeapNode heapnode;
            /* Initialze data*/
            heapnode.index = 0; heapnode.data = 0;   //O(1)
            visited[0] = false;                       //O(1)
            parent[0] = -1;                         //O(1)
            minheap.Add(heapnode);               //O(Log(d))

            for (int i = 1; i < count_dis; i++)      //O(d) * loop body ---> O(dLog(d))
            {
                distance[i] = int.MaxValue - 1;    //O(1) 
                parent[i] = -1;                  //O(1)  
                visited[i] = false;              //O(1)
                heapnode.index = i;              //O(1)
                heapnode.data = distance[i];     //O(1)
                minheap.Add(heapnode);           //O(Log(n)
            }
            /****************************************/
            while (!minheap.IsEmpty()) //O(1)*loopbody ------>O(n^2)       
            {
                heapnode = minheap.Pop();   //O(Log(n))
                visited[heapnode.index] = true; //O(1)

                for (int i = 0; i < count_dis; i++) //O(n)*loopbody  ------->O(n)
                {
                    if (visited[i] == false && distance[i] > FullyConnectedGraph[heapnode.index, i])
                    {
                        HeapNode node;       //O(1)
                        node.index = i;     //O(1)
                        node.data = FullyConnectedGraph[heapnode.index, i];

                        minheap.update(node); //O(n)

                        distance[i] = FullyConnectedGraph[heapnode.index, i]; //O(1)
                        parent[i] = heapnode.index;                           //O(1)
                    }
                }
            }
            MST_Sum = 0;
            for (int i = 0; i < count_dis; i++)  //O(n)
                MST_Sum += distance[i];
        }
    
        public static void Make_clusters() //O(KD)
        {
            MinHeap minheap = new MinHeap(count_dis);
            HeapNode heapnode;
            int myclusters = count_dis;
            clustersNo = new int[count_dis];

            /*****initialize********/
            for (int i = 1; i < count_dis; i++) //O(d)*body -->O(dlog(d))
            {
                heapnode.data = distance[i];         //O(1)
                heapnode.index = i;                 //O(1)
                minheap.Add(heapnode);             //O(LOG(d))
                //clustersNo[i-1] = i-1;//zerobased //O(1)
            }//////--->>>>>
            for (int i = 0; i < count_dis; i++) //O(1)
                clustersNo[i] = i;              //O(1)
            /*********************************/
            int temp; //hold index
            while (myclusters > Kclusters) //O(K)*body -->O(KD)
            {
                heapnode = minheap.Pop();  //O(log(d))
                //for the smallest distance
                temp = clustersNo[heapnode.index];//O(1)
                //make the color cluster with it's parent cluster
                clustersNo[heapnode.index] = clustersNo[parent[heapnode.index]]; //O(1)
                myclusters--;               //O(1)
                //also change it's children
                for(int i=0 ; i<count_dis;i++) //O(d)*body -->O(d)
                    if(clustersNo[i] == temp)
                        clustersNo[i] = clustersNo[heapnode.index]; //O(1)
            }
            
        }
        
        public static void Make_clusters_data() //O(D) 
        {
            clustersData = new RGBPixelD[count_dis];        
            clusters_count = new int[count_dis];
            
            for (int i = 0; i < count_dis; i++) //O(d)*body -->O(d)
            {
                int x = clustersNo[i]; //get index only
                if (clusters_count[x] != 1)
                {
                    clustersData[x].red = distinct_colors[i].red; //O(1)
                    clustersData[x].green = distinct_colors[i].green;//O(1)
                    clustersData[x].blue = distinct_colors[i].blue;//O(1)
                    clusters_count[x] = 1;//O(1)
                }
                else
                {
                    clustersData[x].red += distinct_colors[i].red;//O(1)
                    clustersData[x].green += distinct_colors[i].green;//O(1)
                    clustersData[x].blue += distinct_colors[i].blue;//O(1)
                    clusters_count[x]++;//O(1)
                }
            }
        }

        public static void convert_photo(RGBPixel[,] Imagedata,int height , int width) //O(n^2)
        {
            int index,clusno;
            double pross;
           for(int i =0 ;i<height ; i++) //O(n)*body-->O(n^2)
               for (int j = 0; j < width; j++)//O(n)*body -->O(n)
               {
                   index = dic[Imagedata[i,j]]; //O(1)
                   clusno = clustersNo[index];  //O(1)
                   pross = clustersData[clusno].red / clusters_count[clusno];//O(1)
                   Imagedata[i, j].red = Convert.ToByte(pross);//O(1)
                   pross = clustersData[clusno].green / clusters_count[clusno];//O(1)
                   Imagedata[i, j].green = Convert.ToByte(pross);//O(1)
                   pross = clustersData[clusno].blue / clusters_count[clusno];//O(1)
                   Imagedata[i, j].blue = Convert.ToByte(pross);//O(1)
               }
        
        }


        


    }///////end of processing class

}
