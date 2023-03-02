using System;
using UnityEngine;
using System.Collections.Generic;
using Scripts;

public class BoundingBox
{
    public string id;

    public int width;
    public int height;

    public int minX;
    public int minY;
    public int maxX;
    public int maxY;

    public int buffer { get; private set; }

    // if greater than 0 store the buffered bounding box
    public BoundingBox bufferedBB { get; private set; }

    /// <summary>
    /// Get the corners of a box
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="minY"> </param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <param name="buffer">the padding around the box. A buffer of 1 includes all tiles bordering the player's plot</param>
    public BoundingBox(int minX, int minY, int maxX, int maxY, int buffer = 0, string id = null)
    {
        this.id = id;
        this.buffer = buffer;

        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;

        setWidth();
        setHeight();
        
        setBufferedBB();
        //if (bufferedBB != null)
        //{
        //    Debug.Log("buffer xmin x " + bufferedBB.minX + " min y " + bufferedBB.minY + " max x " + bufferedBB.maxX + " max y " + maxY);
        //}
    }

    /// <summary>
    /// Set the buffered bounding box given the buffer surrounding the original bounding box
    /// </summary>
    /// <param name="id">id of the object the bounding box associated with</param>
    private void setBufferedBB()
    {
        if (buffer > 0) {
            bufferedBB = new BoundingBox(this.minX-buffer, this.minY-buffer, this.maxX+buffer, this.maxY+buffer);
            Map.addBufferedTiles(this);
        }
    }

    /// <summary>
    /// Add extra tiles to either side of a dimension that total padding amount
    /// </summary>
    /// <param name="minDim">The lower bound of the dimension</param>
    /// <param name="maxDim">The upper bound of the dimension</param>
    /// <param name="length">The current length of the dimension</param>
    /// <param name="paddedLength">The length after adding padding</param>
    private void padDimensions(ref int minDim, ref int maxDim, int length, int paddedLength)
    {
        int diff = paddedLength - length;
        int halfDiff = diff % 2 == 0 ? (diff / 2) : (diff / 2 + 1);
        minDim -= halfDiff;
        maxDim += halfDiff;
    }

    /// <summary>
    /// Adjust y coords to meet the target height
    /// </summary>
    /// <param name="targetHeight">The target height of box</param>
    public void increaseBoxHeight(int targetHeight)
    {
        if (targetHeight > height)
        {
            padDimensions(ref minY, ref maxY, height, targetHeight);
            setHeight();
        }
    }

    /// <summary>
    /// Adjust the x coords to meet the target width
    /// </summary>
    /// <param name="targetWidth">The target width of the box</param>
    public void increaseBoxWidth(int targetWidth)
    {
        if (targetWidth > width)
        {
            padDimensions(ref minX, ref maxX, width, targetWidth);
            setWidth();
        }
    }

    /// <summary>
    /// Get the tiles within the bounding box's buffer
    /// </summary>
    /// <returns></returns>
    public List<Vector2Int> getBufferedLocs()
    {
        List<Vector2Int> bufferedLocList = new List<Vector2Int>();
        if (this.bufferedBB != null)
        {
            for (int r = this.bufferedBB.minY; r <= this.bufferedBB.maxY; r++)
            {
                for (int c = this.bufferedBB.minX; c <= this.bufferedBB.maxX; c++)
                {
                    bufferedLocList.Add(new Vector2Int(c, r));
                }
            }
        }
        return bufferedLocList;
    }

    /// <summary>
    /// Get locations that are adjacent to the buffered box
    /// </summary>
    /// <param name="buffer">Amount of spacing separating plot tiles from new tile</param>
    /// <param name="plot">Least connected plot</param>
    /// <returns></returns>
    public List<Vector2Int> getAdjPlotLocs()
    {
        List<Vector2Int> adjPlotLocs = new List<Vector2Int>();

        if (bufferedBB != null)
        {
            // include tiles clockwise starting from the top side

            Vector2Int topLeftLoc = new Vector2Int(bufferedBB.minX - 1, bufferedBB.maxY + 1);
            for (int t = 0; t < bufferedBB.width; t++)
            {
                adjPlotLocs.Add(new Vector2Int(topLeftLoc.x + t, topLeftLoc.y));
            }
            Vector2Int topRightLoc = new Vector2Int(bufferedBB.maxX + 1, bufferedBB.maxY + 1);
            for (int s = 1; s < bufferedBB.height; s++)
            {
                adjPlotLocs.Add(new Vector2Int(topRightLoc.x, topRightLoc.y - s));
            }
            Vector2Int bottomRightLoc = new Vector2Int(bufferedBB.maxX + 1, bufferedBB.minY - 1);
            for (int d = 1; d < bufferedBB.width; d++)
            {
                adjPlotLocs.Add(new Vector2Int(bottomRightLoc.x - d, bottomRightLoc.y));
            }
            Vector2Int bottomLeftLoc = new Vector2Int(bufferedBB.minX - 1, bufferedBB.minY - 1);
            for (int e = 1; e < bufferedBB.height - 1; e++)
            {
                adjPlotLocs.Add(new Vector2Int(bottomLeftLoc.x, bottomLeftLoc.y + e));
            }
        }
        return adjPlotLocs;
    }

    public void setWidth()
    {
        width = maxX - minX + 1;
    }

    public void setHeight()
    {
        height = maxY - minY + 1;
    }

    public void setMinX(int minX)
    {
        this.minX = minX;
        this.setWidth();
        this.setBufferedBB();
    }

    public void setMaxX(int maxX)
    {
        this.maxX = maxX;
        this.setWidth();
        this.setBufferedBB();        
    }

    public void setMinY(int minY)
    {
        this.minY = minY;
        this.setHeight();
        this.setBufferedBB();        
    }

    public void setMaxY(int maxY)
    {
        this.maxY = maxY;
        this.setHeight();
        this.setBufferedBB();        
    }
}
