using System;
using System.Collections;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;

namespace MVC_S3_Helper.Helpers
{
    public class AwsS3Helper
    {
        #region Fields
        private static AmazonS3Config _awss3Config;
        private static AmazonS3Client _awess3Client;
        public static ArrayList S3BucketsList;
        public static ArrayList S3ObjectKeyList;
        public static int S3BucketsListCounter;
        public static ArrayList S3FolderList;
        public static int S3FolderListCounter;
        public static string BaseUrl = string.Empty;        
        public static int RegionCounterS3 = 13;
        public static int RegionDefaultCounterS3 = 13;
        #endregion

        #region Connect to AWS S3
        public bool ConnectS3(string myAccessKeyId, string mySecretAccessKey, string myRegion)
        {
            try
            {
                _awss3Config = new AmazonS3Config { ServiceURL = "https://" + GetS3RegionEndPoint(myRegion) };
                _awess3Client = new AmazonS3Client(myAccessKeyId, mySecretAccessKey, _awss3Config);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ConnectS3(string myAccessKeyId, string mySecretAccessKey, string myRegion, string bucketName)
        {
            try
            {
                BaseUrl = "https://" + bucketName + "." + GetS3RegionEndPoint(myRegion);
                _awss3Config = new AmazonS3Config { ServiceURL = "https://" + GetS3RegionEndPoint(myRegion) };
                _awess3Client = new AmazonS3Client(myAccessKeyId, mySecretAccessKey, _awss3Config);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Create AWS S3 Bucket
        public bool CreateBucket(string bucketName, string bucketRegion)
        {
            try
            {
                var request = new PutBucketRequest { BucketName = bucketName };
                switch (bucketRegion)
                {
                    case "US East (Northern Virginia)":
                        request.BucketRegion = S3Region.US;
                        break;
                    case "US West (Oregon)":
                        request.BucketRegion = S3Region.USW2;
                        break;
                    case "US West (Northern California)":
                        request.BucketRegion = S3Region.USW1;
                        break;
                    case "EU (Ireland)":
                        request.BucketRegion = S3Region.EU;
                        break;
                    case "EU (Frankfurt)":
                        request.BucketRegion = S3Region.EUC1;
                        break;
                    case "Asia Pacific (Singapore)":
                        request.BucketRegion = S3Region.APS1;
                        break;
                    case "Asia Pacific (Sydney)":
                        request.BucketRegion = S3Region.APS2;
                        break;
                    case "Asia Pacific (Tokyo)":
                        request.BucketRegion = S3Region.APN1;
                        break;
                    case "South America (Sao Paulo)":
                        request.BucketRegion = S3Region.SAE1;
                        break;
                    case "China (Beijing)":
                        request.BucketRegion = S3Region.US;
                        break;
                    case "AWS GovCloud (US)":
                        request.BucketRegion = S3Region.US;
                        break;
                    case "Korea (Seoul)":
                        request.BucketRegion = S3Region.APN2;
                        break;
                    case "India (Mumbai)":
                        request.BucketRegion = S3Region.APS3;
                        break;
                }
                _awess3Client.PutBucket(request);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Get List of AWS S3 Buckets
        public ArrayList GetListBuckets()
        {
            S3BucketsList = new ArrayList();
            try
            {
                var response = _awess3Client.ListBuckets();
                S3BucketsListCounter = 0;
                foreach (var awsBucket in response.Buckets)
                {
                    S3BucketsList.Add(awsBucket.BucketName);
                    S3BucketsListCounter = S3BucketsListCounter + 1;
                }
                return S3BucketsList;
            }
            catch (Exception)
            {
                return new ArrayList();
            }
        }

        #endregion

        #region Get List of AWS S3 Folders
        public ArrayList GetListFolders(string bucketName)
        {
            S3FolderList = new ArrayList();
            try
            {
                var request = new ListObjectsRequest { BucketName = bucketName };

                while (request != null)
                {
                    ListObjectsResponse response = _awess3Client.ListObjects(request);

                    foreach (var entry in response.S3Objects)
                    {
                        S3FolderList.Add(entry.Key);
                        S3FolderListCounter = S3FolderListCounter + 1;
                    }

                    if (response.IsTruncated)
                    {
                        request.Marker = response.NextMarker;
                    }
                    else
                    {
                        request = null;
                    }

                }

                return S3FolderList;
            }
            catch (Exception)
            {
                return new ArrayList();
            }
        }

        /// <summary>
        /// Get List of Object Keys in a folder
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>

        public ArrayList GetListKeys(string bucketName, string folderName)
        {
            S3ObjectKeyList = new ArrayList();
            try
            {
                var request = new ListObjectsRequest { BucketName = bucketName, Prefix = folderName };

                while (request != null)
                {
                    ListObjectsResponse response = _awess3Client.ListObjects(request);

                    foreach (var entry in response.S3Objects)
                    {
                        S3ObjectKeyList.Add(entry.Key);
                    }

                    if (response.IsTruncated)
                    {
                        request.Marker = response.NextMarker;
                    }
                    else
                    {
                        request = null;
                    }

                }

                return S3ObjectKeyList;
            }
            catch (Exception)
            {
                return new ArrayList();
            }
        }

        #endregion

        #region Get AWS S3 Region EndPoint
        public string GetS3RegionEndPoint(string awsRegion)
        {
            var result = string.Empty;

            foreach(var regionS3 in GetS3RegionList())
            {
                if (regionS3.RegionName.ToUpper() == awsRegion.ToUpper())
                {
                    result = regionS3.Endpoint;
                    break;
                }
            }
            return result;
        }
        #endregion

        #region Create AWS S3 Folder
        public bool CreateFolder(string bucketName, string folderName)
        {
            try
            {
                dynamic folderKey = folderName + "/";
                dynamic request = new PutObjectRequest();
                request.BucketName = bucketName;
                request.StorageClass = S3StorageClass.Standard;
                request.ServerSideEncryptionMethod = ServerSideEncryptionMethod.None;
                request.Key = folderKey;
                request.ContentBody = string.Empty;
                _awess3Client.PutObject(request);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Upload AWS S3 File     

        /// <summary>
        /// This function was modified by lisan.sojib
        /// It uploads an object from AWS S3 
        /// </summary>
        /// <param name="s3FileName">Full file path</param>
        /// <param name="s3Bucket">AWS S3 Bucket name</param>
        /// <param name="s3Folder">AWS s3 Folder name</param>
        /// <param name="contentType">Type of the file</param>
        /// <param name="inputStream">Input Stream</param>
        /// <returns>file url</returns>
        public bool UploadS3File(string s3ObjectKey, string s3Bucket, string contentType, Stream inputStream)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    Key = s3ObjectKey,
                    BucketName = s3Bucket,
                    CannedACL = S3CannedACL.PublicRead,
                    ContentType = contentType,
                    InputStream = inputStream
                };

                _awess3Client.PutObject(request);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Upload multipart file
        /// </summary>
        /// <param name="s3File"></param>
        /// <param name="s3Bucket"></param>
        /// <param name="s3Folder"></param>
        /// <returns>Returns True if operation successful, False otherwise.</returns>
        public bool UploadS3MultipartFile(string s3File, string s3Bucket, string s3Folder)
        {
            try
            {
                var s3FileName = Path.GetFileName(s3File);
                var request = new PutObjectRequest
                {
                    Key = s3Folder + "/" + s3FileName,
                    BucketName = s3Bucket,
                    CannedACL = S3CannedACL.PublicRead
                };
                _awess3Client.PutObject(request);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Retrive AWS S3 File

        /// <summary>
        /// It retrives an AWS S3 object.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="s3Bucket"></param>
        /// <param name="s3Folder"></param>
        /// <returns>Returns stream representation of the object.</returns>
        public Stream GetS3FileStream(string fileName, string s3Bucket, string s3Folder)
        {
            try
            {

                Stream imageStream = new MemoryStream();
                var request = new GetObjectRequest
                {
                    BucketName = s3Bucket,
                    Key = s3Folder + "/" + fileName
                    //Key = "zaman/artwork/525a0080-fd94-40e9-8237-756a379ce123-cropped"
                };
                using (GetObjectResponse response = _awess3Client.GetObject(request))
                {
                    response.ResponseStream.CopyTo(imageStream);
                }
                imageStream.Position = 0;

                return imageStream;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// It retrives an AWS S3 object.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="s3Bucket"></param>
        /// <param name="s3Folder"></param>
        /// <returns>Returns stream representation of the object.</returns>
        public string GetS3FileBase64(string s3ObjectKey, string s3Bucket)
        {
            string imageStr;
            try
            {
                var imageStream = new MemoryStream();
                var request = new GetObjectRequest
                {
                    BucketName = s3Bucket,
                    Key = s3ObjectKey
                };
                using (GetObjectResponse response = _awess3Client.GetObject(request))
                {
                    response.ResponseStream.CopyTo(imageStream);

                    var bytes = imageStream.ToArray();
                    imageStr = "data:" + response.Headers.ContentType + ";base64," + Convert.ToBase64String(bytes);
                }

                return imageStr;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// This functions was added by lisan.sojib
        /// </summary>
        /// <param name="fileName">Name of the File</param>
        /// <returns>Returns Pre-signed url of the file if the file is found.</returns>
        public string GetS3FileUrl(string s3ObjectKey)
        {
            try
            {
                var url = BaseUrl + "/" + s3ObjectKey;
                return url;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region Delete AWS S3 object / file
        /// <summary>
        /// Deletes a single object from AWS S3
        /// </summary>
        /// <param name="objectKey">AWS S3 object key</param>
        /// <param name="bucketName">AWS S3 Bucket name</param>
        /// <param name="versionId">Version Id of the object</param>
        /// <returns>Returns True if delete successful, False otherwise.</returns>
        public bool DeleteS3Object(string objectKey, string bucketName, string versionId)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectKey,
                    VersionId = versionId
                };
                _awess3Client.DeleteObjectAsync(request);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes a list of  object from AWS S3
        /// </summary>
        /// <param name="fileName">Name of the file or objcet to delete</param>
        /// <param name="bucketName">AWS S3 bucket name</param>
        /// <param name="s3Folder">AWS S3 Folder name</param>
        /// <param name="versionId">Version Id of the object</param>
        /// <returns></returns>
        public int DeleteS3Objects(string[] objectKeyList, string bucketName, string versionId)
        {
            var totalDeleted = 0;
            try
            {
                foreach (string objectKey in objectKeyList)
                {
                    var request = new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = objectKey,
                        VersionId = versionId
                    };
                    _awess3Client.DeleteObjectAsync(request);

                    totalDeleted++;
                }
                return totalDeleted;
            }
            catch (Exception)
            {
                return totalDeleted;
            }
        }

        #endregion

        #region AWS S3 Helpers
        /// <summary>
        /// Enables Versioning on a AWS S3 Bucket.
        /// </summary>
        /// <param name="bucketName"></param>
        public static void EnableVersoningOnBucket(string bucketName)
        {
            try
            {
                var setBucketVersioningRequest = new PutBucketVersioningRequest
                {
                    BucketName = bucketName,
                    VersioningConfig = new S3BucketVersioningConfig { Status = VersionStatus.Enabled }
                };
                _awess3Client.PutBucketVersioning(setBucketVersioningRequest);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrives the versions of a AWS S3 object.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bucketName"></param>
        /// <param name="s3Folder"></param>
        /// <returns></returns>
        public static ListVersionsResponse GetObjectVersions(string fileName, string bucketName, string s3Folder)
        {
            try
            {
                var listResponse = _awess3Client.ListVersions(new ListVersionsRequest
                {
                    BucketName = bucketName,
                    Prefix = s3Folder + "/" + fileName
                });
                return listResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RegionS3> GetS3RegionList()
        {
            return new List<RegionS3>
            {
                new RegionS3
                {
                    RegionName = "US East (Northern Virginia)",
                    Region = "us-east-1",
                    Endpoint = "s3.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "US West (Oregon)",
                    Region = "us-west-2",
                    Endpoint = "s3-us-west-2.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "US West (Northern California)",
                    Region = "us-west-1",
                    Endpoint = "s3-us-west-1.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "EU (Ireland)",
                    Region = "eu-west-1",
                    Endpoint = "s3-eu-west-1.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "EU (Frankfurt)",
                    Region = "eu-central-1",
                    Endpoint = "s3-eu-central-1.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "Asia Pacific (Singapore)",
                    Region = "ap-southeast-1",
                    Endpoint = "s3-ap-southeast-1.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "Asia Pacific (Sydney)",
                    Region = "ap-southeast-2",
                    Endpoint = "s3-ap-southeast-2.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "Asia Pacific (Tokyo)",
                    Region = "ap-northeast-1",
                    Endpoint = "s3-ap-northeast-1.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "South America (Sao Paulo)",
                    Region = "sa-east-1",
                    Endpoint = "s3-sa-east-1.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "China (Beijing)",
                    Region = "cn-north-1",
                    Endpoint = "s3-cn-north-1.amazonaws.com.cn",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "AWS GovCloud (US)",
                    Region = "us-gov-west-1",
                    Endpoint = "s3-us-gov-west-1.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "Korea (Seoul)",
                    Region = "ap-northeast-2",
                    Endpoint = "s3-ap-northeast-2.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },

                new RegionS3
                {
                    RegionName = "India (Mumbai)",
                    Region = "ap-south-1",
                    Endpoint = "s3-ap-south-1.amazonaws.com",
                    Protocol = "HTTPS",
                    RegionPublic = true
                },
            };
        }

        #endregion
    }

    public class RegionS3
    {
        public string RegionName { get; set; }
        public string Region { get; set; }
        public string Endpoint { get; set; }
        public string Protocol { get; set; }
        public bool RegionPublic { get; set; }
    }
}
