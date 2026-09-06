using UnityEngine.Networking;

namespace Game2
{
	public class BypassCertificateHandler : CertificateHandler
	{
		protected override bool ValidateCertificate(byte[] certificateData)
		{
			return true;
		}
	}
}
