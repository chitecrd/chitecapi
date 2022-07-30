using System.Net.Security;

 using System.Security.Cryptography.X509Certificates;

    

class Certificates

{

    public static bool ValidateRemoteCertificate(object sender,

                                                X509Certificate certificate,

                                                X509Chain chain,

                                                SslPolicyErrors policyErrors)

{

            //Return True to force the certificate to be accepted.     

    //Needed so that calling web services with self-signed certs will work.

    return true;

        }

}