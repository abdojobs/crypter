THIS PROJECT HAS NOW MIGRATED TO CODEPLEX https://crypter.codeplex.com/

crypter is a small utility to encode-decode/encrypt-decrypt any file with Advanced Encryption Standard (AES) or Base64 algorithm.

This project has been developed in C Sharp. Console application under .NET Framework 2.0 compatible with [MonoDevelop](http://monodevelop.com/).


---


**Usage**

> crypter [options...] input-file-name

_Options:_

  * -m  --mode: b64 (Base 64) or AES (Advanced Encryption Standard).
  * -e  --encrypt-encode: Encrypt or encode operation indicator.
  * -d  --decrypt-decode: Decrypt or decode operation indicator.
  * -k  --key-size: For AES only. 128, 192, or 256 (By default).
  * -p  --password: For AES only. Word or phrase. Required parameter.
  * -s  --salt: For AES only. By default: "PkZrST6".
  * -a  --algorithm: For AES only. MD5 or SHA1 (By default).
  * -i  --initial-vector: For AES only. Needs to be 16 ASCII characters long (By default: "AgTxp96`*`Zf8e12Xy").
  * -o  --output-file: Output file name.
  * -w  --overwrite: Overwrites the existing output file(s) without asking.

_Samples:_

  * crypter -o myfile.b64 -m b64 -e myfile.txt
  * crypter -o myfile.txt -m b64 -d myfile.b64
  * crypter -o myfile.aes -m aes -p "my password" -e myfile.bin
  * crypter -o myfile.bin -m aes -p "my password" -d myfile.aes
  * crypter -o myfile.aes -m aes -e myfile.bin
  * crypter -o myfile.bin -m aes -d myfile.aes