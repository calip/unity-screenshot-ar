using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Screenshot : MonoBehaviour
{
	// The folder we place all screenshots inside.
	// If the folder exists we will append numbers to create an empty folder.
	public string folder = "ScreenshotMovieOutput";
	public int frameRate = 25;
	public int sizeMultiplier = 1;
	
	private string realFolder = "";
	private int framecount = 10;

	string predefinedPathBase;
	public string targetDir = "";

	public string sender = "";
	public string password = "";
	public string recepient = "";

	public bool sentStatus = false;

	void Start()
	{
		predefinedPathBase = Application.streamingAssetsPath + "/Record";
		Time.captureFramerate = frameRate;
	}
	
	void Update()
	{
		realFolder = predefinedPathBase + "/"+targetDir;
		// name is "realFolder/shot 0005.png"
		var name = string.Format("{0}/shot.png", realFolder, framecount);
		
		// Capture the screenshot
		Application.CaptureScreenshot(name, sizeMultiplier);

		if (FileSystemUtil.CheckDir (realFolder)) {
			SendEmail ();
		}
	}

	public void SendEmail(){
		List<string> files = FileSystemUtil.FindFilesRecursively (Application.streamingAssetsPath, "*");
		if(files.Count > 0){
			if(!sentStatus){

				string FilePath = "";
				string AttachmentName = "shot.png";
				string FileName = "";
				
				#if UNITY_EDITOR
				FilePath = string.Format("@"+realFolder+"/{0}", AttachmentName);
				#else
				FilePath = Application.persistentDataPath + "/" + AttachmentName;
				if(!File.Exists(FilePath)) {
					WWW loadImage = new WWW("jar:file://" + Application.dataPath + "!/assets/" + AttachmentName);
					while(!loadImage.isDone) {}
					File.WriteAllBytes(FilePath, loadImage.bytes);
				}
				#endif
				
				FileName = realFolder+"/shot.png";
				
				MailMessage mail = new MailMessage();
				
				mail.From = new MailAddress(sender);
				mail.To.Add(recepient);
				mail.Subject = "AR Attachment";
				mail.Body = "AR Dunia Kutub";
				
				Attachment data = new Attachment(FileName, System.Net.Mime.MediaTypeNames.Application.Octet);
				// Add time stamp information for the file.
				System.Net.Mime.ContentDisposition disposition = data.ContentDisposition;
				disposition.CreationDate = System.IO.File.GetCreationTime(FileName);
				disposition.ModificationDate = System.IO.File.GetLastWriteTime(FileName);
				disposition.ReadDate = System.IO.File.GetLastAccessTime(FileName);
				
				mail.Attachments.Add(data);
				
				SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
				smtpServer.Port = 587;
				smtpServer.Credentials = new System.Net.NetworkCredential (sender, password) as ICredentialsByHost;
				smtpServer.EnableSsl = true;
				ServicePointManager.ServerCertificateValidationCallback = 
				delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
					return true;
				};
				try {
					smtpServer.Send(mail);
					sentStatus = true;
				} catch (Exception e) {
					Debug.Log (e.GetBaseException ());
				}
			}
		}

	}

}
