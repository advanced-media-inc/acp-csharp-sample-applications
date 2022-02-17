# SpeakerDiarizationSampleApp
AmiVoice Cloud Platformの非同期HTTP音声認識APIにおける話者ダイアライゼーションを使用したWindowsアプリ

## About
複数人が話している会議の録音した音声ファイルをアップロードするだけで、自動的に発言者ごとに区別して、文字起こしすることができます。

<div align="center" position="relative">
  <a href="#">
      <img src="https://user-images.githubusercontent.com/93509335/154015326-abf6f419-dfb1-4c1e-a4aa-e50fcd4db625.png"
     width="600px">
  </a>
  <p>図1 サンプルアプリのイメージ図</p>
</div>

## Article
本Windowsアプリは、下記のテックブログで詳しくご紹介しております。良ければご覧ください。

[【HttpClient】C#でAmiVoiceの話者ダイアライゼーションを利用する方法](https://amivoice-tech.hatenablog.com/draft/entry/-IcQWJw05S6sXdMZfU_vHqCWR88)

## Requirements
- Windows 10
- Visual Studio 2019
- .NET 5.0

## Reference
- [マニュアル Archive - AmiVoice Cloud Platform](https://acp.amivoice.com/main/manual/)
- [HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-6.0)
- [Multipart Form Post in C#](https://briangrinstead.com/blog/multipart-form-post-in-c/)
- [HttpClient でファイルアップロード](http://surferonwww.info/BlogEngine/post/2019/08/11/file-upload-by-using-httpclient.aspx)
- [【C#】System.Net.Http.HttpClientを使ってWeb APIとHTTP通信してみよう](https://iwasiman.hatenablog.com/entry/20210622-CSharp-HttpClient)
- [【C#】HttpClientを使ってみる（POST）](https://husk.hatenablog.com/entry/2018/07/24/231738)
