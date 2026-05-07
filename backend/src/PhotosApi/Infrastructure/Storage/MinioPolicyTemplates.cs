using PhotosApi.Helpers;

namespace PhotosApi.Infrastructure.Storage;

public static class MinioPolicyTemplates
{
    public static string GetPhotoBucketPolicy(string bucketName) => $$"""
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Principal": { "AWS": ["*"] },
                "Action": ["s3:GetObject" ],
                "Resource": [
                    "arn:aws:s3:::{{bucketName}}/gallery/*",
                    "arn:aws:s3:::{{bucketName}}/homepage/*",
                    "arn:aws:s3:::{{bucketName}}/thumbnails/*"
                ]
            },
            {
                "Effect": "Deny",
                "Principal": { "AWS": ["*"] },
                "Action": ["s3:GetObject" ],
                "Resource": [
                    "arn:aws:s3:::{{bucketName}}/uploads/*",
                    "arn:aws:s3:::{{bucketName}}/archived/*"
                ]
            }
        ]
    }                                                                  
    """;


}