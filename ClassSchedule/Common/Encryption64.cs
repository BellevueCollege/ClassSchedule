using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace CTCClassSchedule.Common
{
	public class Encryption64
	{
		protected byte[] key;
		protected byte[] IV = {0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF};

		public string Decrypt(string stringToDecrypt, string sEncryptionKey) {

			byte[] inputByteArray = new byte[stringToDecrypt.Length];

			try {
				key = Encoding.UTF8.GetBytes(sEncryptionKey.Substring(0, 8));
				DESCryptoServiceProvider des = new DESCryptoServiceProvider();
				inputByteArray = Convert.FromBase64String(stringToDecrypt);
				MemoryStream ms = new MemoryStream();
				CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
				cs.Write(inputByteArray, 0, inputByteArray.Length);
				cs.FlushFinalBlock();
				return System.Text.Encoding.UTF8.GetString(ms.ToArray());
			}
			catch (Exception e) {
				return e.Message;
			}
		}

		public string Encrypt(string stringToEncrypt, string SEncryptionKey) {
			try {
				key = Encoding.UTF8.GetBytes(SEncryptionKey.Substring(0, 8));
				DESCryptoServiceProvider des = new DESCryptoServiceProvider();
				byte[] inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
				MemoryStream ms = new MemoryStream();
				CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
				cs.Write(inputByteArray, 0, inputByteArray.Length);
				cs.FlushFinalBlock();
				return Convert.ToBase64String(ms.ToArray());

			}
			catch (Exception e) {
				return e.Message;
			}
		}
	}
}
