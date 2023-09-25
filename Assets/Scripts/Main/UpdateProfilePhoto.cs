using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using System.Threading.Tasks;
using Firebase.Extensions;
using static Post;
using static CommunityDAO;

public class UpdateProfilePhoto : MonoBehaviour
{
    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    System.Uri Photo_url;
    DatabaseReference reference;
    StorageReference storageReference;
    string imageURL;
    string imageName = "";
    public TMP_Text Message;
    
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        CommunityDAO dao = new CommunityDAO();
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(dao.getReferenceURL());
        Debug.Log(dao.getReferenceURL());
        Message.text = "ImageName: " + imageName;
    }
    
    void Save()
    {
        if (imageName == "") {
            Message.text = "No image selected";
            return;
        }

        if (user == null) {
            Message.text = "You need to login first";
            return;
        }

        string localFile = "./Assets/ExportedPng/" + imageName + ".png", imageURL = user.UserId + ".png";
        StorageReference imageRef = storageReference.Child("profile").Child(imageURL);
        // upload new profile photo
        imageRef.PutFileAsync(localFile).ContinueWith((Task<StorageMetadata> task) => {
            if (task.IsFaulted || task.IsCanceled) {
                Debug.Log(task.Exception.ToString());
            }
            else {
                Debug.Log("Finished uploading...");

                // Change imageURL to result download URL.
                Photo_url = imageRef.GetDownloadUrlAsync().Result;
                Debug.Log(Photo_url); // Download URL

                Debug.Log("Upload complete");

                // update user profile
                Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile {
                    DisplayName = user.DisplayName,
                    PhotoUrl = Photo_url,
                };

                user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                    if (task.IsCanceled) {
                        Debug.LogError("UpdateUserProfileAsync was canceled.");
                        return;
                    }
                    if (task.IsFaulted) {
                        Debug.LogError("UpdateUserProfileAsync has error: " + task.Exception);
                        return;
                    }

                    Message.text = "User profile updated successfully.";
                });
            }
        });
        StartCoroutine(UploadWait());
    }

    IEnumerator UploadWait() 
    {
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene("Profile");
    }

    public void UpdateImgURL(string name)
    {
        imageName = name;
        Message.text = "ImageName: " + imageName;
    }

    public void UploadClick() 
    {
        Save();
    }

    public void BackClick()
    {
        SceneManager.LoadScene("CommunityMain");
    }
}