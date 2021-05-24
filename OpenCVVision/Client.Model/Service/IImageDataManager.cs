﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

using Client.Model.Entity;

using DynamicData;

using OpenCvSharp;

namespace Client.Model.Service
{
    public interface IImageDataManager
    {
        Guid CurrentId { set; get; }
        Subject<Guid> InputMatGuid { set; get; }
        Subject<Mat> OutputMat { set; get; }
        SourceCache<ImageData, Guid> SourceCacheImageData { set; get; }

        bool AddImage(string imageMarkTxt, Mat mat);

        ImageData GetImage(Guid guid);

        bool RemoveIamge(string imageMarkTxt);

        bool RemoveImage(Guid guid);
    }
}