using UnityEngine;
using System.Collections;
using System.IO;
using System;
using SimpleFileBrowser;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;

public class FileManager : MonoBehaviour
{
	static string url = "http://127.0.0.1:8000/upload/";
	static HttpClient client = new HttpClient()
    {
		BaseAddress = new(url)
    };

	// Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
	// Warning: FileBrowser can only show 1 dialog at a time

	public void ShowFileBrowser()
	{
		// Set filters (optional)
		// It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
		// if all the dialogs will be using the same filters
		FileBrowser.SetFilters(false, new FileBrowser.Filter("MIDI Files", ".mid", ".midi"));

		// Set default filter that is selected when the dialog is shown (optional)
		// Returns true if the default filter is set successfully
		// In this case, set Images filter as the default filter
		FileBrowser.SetDefaultFilter(".mid");

		// Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
		// Note that when you use this function, .lnk and .tmp extensions will no longer be
		// excluded unless you explicitly add them as parameters to the function
		FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

		// Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
		// It is sufficient to add a quick link just once
		// Name: Users
		// Path: C:\Users
		// Icon: default (folder icon)
		FileBrowser.AddQuickLink("Users", "C:\\Users", null);

		// Show a save file dialog 
		// onSuccess event: not registered (which means this dialog is pretty useless)
		// onCancel event: not registered
		// Save file/folder: file, Allow multiple selection: false
		// Initial path: "C:\", Initial filename: "Screenshot.png"
		// Title: "Save As", Submit button text: "Save"
		// FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );

		// Show a select folder dialog 
		// onSuccess event: print the selected folder's path
		// onCancel event: print "Canceled"
		// Load file/folder: folder, Allow multiple selection: false
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Select Folder", Submit button text: "Select"
		// FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
		//						   () => { Debug.Log( "Canceled" ); },
		//						   FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select" );

		// Coroutine example
		StartCoroutine(ShowLoadDialogCoroutine());
	}

	IEnumerator ShowLoadDialogCoroutine()
	{
		// Show a load file dialog and wait for a response from user
		// Load file/folder: both, Allow multiple selection: true
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Load File", Submit button text: "Load"
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, false, null, null, "Load MIDI Files", "Load");

		// Dialog is closed
		// Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
		//Debug.Log(FileBrowser.Success);

		if (FileBrowser.Success)
		{
			// Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
			/*
			for (int i = 0; i < FileBrowser.Result.Length; i++)
				Debug.Log(FileBrowser.Result[i]);
			*/

			// Read the bytes of the first file via FileBrowserHelpers
			// Contrary to File.ReadAllBytes, this function works on Android 10+, as well
			MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
			for (int i = 0; i < 1 /*FileBrowser.Result.Length*/; i++)
			{
				/*
				var fileContent = new StreamContent(File.OpenRead(FileBrowser.Result[i]))
				{
					Headers =
					{
						ContentType = new MediaTypeHeaderValue("audio/midi")
					}
				};
				var request = new HttpRequestMessage(HttpMethod.Post, "");
				var content = new MultipartFormDataContent
				{
					{ fileContent, "file", Path.GetFileName(FileBrowser.Result[i]) }
				};

				request.Content = content;
				Task t = PreprocessMidi(client, request);
				t.ContinueWith(_ => { Debug.Log("Task completed!"); });
				*/

				int v = PianoRoll.pr.musicDropdown.options.FindIndex(e => Path.GetFileNameWithoutExtension(FileBrowser.Result[i]).Equals(e.text));
				if (v != -1)
                {
					PianoRoll.pr.musicDropdown.value = v;
					PianoRoll.pr.ChangeMusic();
					yield break;
                }

				byte[] b = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[i]);
				ByteArrayContent byteArrayContent = new ByteArrayContent(b)
				{
					Headers =
					{
						ContentType = new MediaTypeHeaderValue("audio/midi")
                    }
                };
				multipartFormDataContent.Add(byteArrayContent, "file", Path.GetFileName(FileBrowser.Result[i]));
				Task t = PreprocessMidi(client, multipartFormDataContent, Path.GetFileNameWithoutExtension(FileBrowser.Result[i]));
				t.ContinueWith(_ => { /*Debug.Log("Task completed!");*/ });
			}
		}
	}

	async Task PreprocessMidi(HttpClient httpClient, HttpContent httpContent, string filename)
    {
		PianoRoll.pr.IsReady = false;
		try
        {
			using (HttpResponseMessage response = await httpClient.PostAsync(url, httpContent))
			{
				string body = await response.Content.ReadAsStringAsync();

				if (response.StatusCode == HttpStatusCode.OK)
                {
					StartCoroutine(PianoRoll.pr.InitializeWithCustomMidi(body, filename));
                }
				else
                {
					Debug.Log(response.StatusCode);
					Debug.Log(body);
				}
            }
        }
		catch (HttpRequestException e1)
        {
			Debug.LogException(e1);
        }
		catch (Exception e2)
        {
			Debug.LogException(e2);
        }
		finally
        {
			PianoRoll.pr.IsReady = true;
        }
    }

	/*
	async Task PreprocessMidi(HttpClient httpClient, HttpRequestMessage request)
	{
		try
		{
			using (HttpResponseMessage response = await httpClient.SendAsync(request))
			{
				Debug.Log(response.RequestMessage);
				Debug.Log(response.Content.Headers);
				string body = await response.Content.ReadAsStringAsync();
				Debug.Log(body);
				Debug.Log(response.StatusCode);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					PianoRoll.pr.InitializeWithCustomMidi(body);
				}
			}
		}
		catch (HttpRequestException e1)
		{
			Debug.LogException(e1);
		}
		catch (Exception e2)
		{
			Debug.LogException(e2);
		}
	}
	*/
}