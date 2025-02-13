﻿using OpenCVVision.Data;
using OpenCVVision.Model.Entity;

using DynamicData;

using System.Text;

namespace OpenCVVision.Services;

public class ImageDataMemery : IImageDataManager, IEnableLogger
{
    public Guid? CurrentId { get; set; }
    public Subject<Guid?> InputMatGuidSubject { get; set; } = new();
    public Mat OutputMat { get; set; } = new();
    public Subject<Mat> OutputMatSubject { get; set; } = new();
    public SourceCache<ImageData, Guid> SourceCacheImageData { get; set; }

    public ImageDataMemery()
    {
        SourceCacheImageData = new SourceCache<ImageData, Guid>(t => t.ImageId);
        InputMatGuidSubject.Subscribe(guid => CurrentId = guid);
        OutputMatSubject.Subscribe(mat => OutputMat = mat.Clone());
        SampleData();
    }

    private void SampleData()
    {
        Mat mat = Cv2.ImRead(FilePath.Image.Ta);
        AddImage("Sam1", mat);
    }

    public bool AddImage(string imageMarkTxt, Mat mat)
    {
        if (mat != null && !mat.Empty())
        {
            int itemCount = SourceCacheImageData.Items.Count(it => it.TxtMarker.Equals(imageMarkTxt, StringComparison.Ordinal));
            if (itemCount.Equals(0))
            {
                try
                {
                    Guid guid = Guid.NewGuid();
                    SourceCacheImageData.AddOrUpdate(new ImageData { ImageId = guid, TxtMarker = imageMarkTxt, ImageMat = mat });
                    InputMatGuidSubject.OnNext(guid);
                    return true;
                }
                catch (Exception ex)
                {
                    this.Log().Error(ex, ex.Message);
                    return false;
                }
            }
            else
            {
                StringBuilder strBuilderTxtNew = new();
                strBuilderTxtNew.Append(imageMarkTxt);
                strBuilderTxtNew.Append(DateTime.Now.ToLongTimeString());
                return AddImage(strBuilderTxtNew.ToString(), mat);
            }
        }
        else
        {
            return false;
        }
    }

    public void AddOutputImage(string outputImageMarkTxt)
    {
        string str = string.IsNullOrWhiteSpace(outputImageMarkTxt) ? "dst" : outputImageMarkTxt;
        AddImage(str, OutputMat);
    }

    public Mat? GetCurrentMat()
    {
        return CurrentId.HasValue ? GetImage(CurrentId).ImageMat : null;
    }

    public ImageData? GetImage(Guid? guid)
    {
        return guid.HasValue ? SourceCacheImageData.Items.Single(t => t.ImageId.Equals(guid)) : null;
    }

    public bool IsExsitByMark(string markTxt)
    {
        return SourceCacheImageData.Items.Any(it => it.TxtMarker.Contains(markTxt));
    }

    public void RaiseCurrent()
    {
        InputMatGuidSubject.OnNext(CurrentId);
    }

    public bool RemoveCurrentImage()
    {
        if (CurrentId.HasValue)
        {
            bool bol = RemoveImage(CurrentId);

            if (bol)
            {
                CurrentId = SourceCacheImageData.Items.Any() ? SourceCacheImageData.Items?.LastOrDefault()?.ImageId : null;
                InputMatGuidSubject.OnNext(CurrentId);
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    public bool RemoveIamge(string imageMarkTxt)
    {
        ImageData? itemTmp = SourceCacheImageData.Items?.Where(it => it.TxtMarker.Equals(imageMarkTxt, StringComparison.Ordinal)).Select(t => t).FirstOrDefault();
        try
        {
            if (itemTmp is not null)
            {
                SourceCacheImageData.Remove(itemTmp);
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool RemoveImage(Guid? guid)
    {
        try
        {
            SourceCacheImageData.RemoveKey(guid.Value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public ImageData GetImage(string markTxt)
    {
        return SourceCacheImageData.Items.FirstOrDefault(it => it.TxtMarker.Equals(markTxt, StringComparison.Ordinal));
    }
}