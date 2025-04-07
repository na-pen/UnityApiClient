
# UnityApiClient
UnityからAPIを使用するためのプログラム群。プロジェクトの巨大化のため分割。

## 使用方法
### Log
ログを管理するAPIに向けてデータを送信(POST)するためのクラス    

 1. APIを用意する
 Ver0では、logLevel, message, metadata, timestamp をPOSTでAPIに送信します。    
 これらが受信できるAPIを用意してください。
 なお、metadataには次の情報を含みます。
 
|変数名|説明|
|--|--|
|LogVersion|ログのバージョン|
|DeviceId|一意のデバイスID|
|AppVersion|アプリケーションのバージョン|
|ip|IPv4アドレス|
|Platform|プラットフォーム|
|OsVersion|OSのバージョン|
|DeviceModel|デバイスのモデル|
|ScreenResolution|画面解像度|
|Language|言語|    
 
 2. 関数を実行する
    実装している関数は次のとおりです。
    なお、それぞれの関数実行時、UnityEngine.Debug.Logなどが実行されます。
        
|関数|説明|
|--|--|
|Debug(string message)|デバッグログを送信する|
|Info(string message)|インフォログを送信する|
|Warning(string message)|ワーニングログを送信する|
|Error(string message)|エラーログを送信する|


 
