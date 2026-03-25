using Razorpay.Api;
using Microsoft.Extensions.Configuration;
using motomart_BE.Models;
using motomart_BE.Data;
using Microsoft.EntityFrameworkCore;

namespace motomart_BE.Services
{
    public interface IPaymentService
    {
        string CreateOrder(decimal amount, string receiptId);
        bool VerifyPayment(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly string _keyId;
        private readonly string _keySecret;

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
            _keyId = _configuration["Razorpay:KeyId"]!;
            _keySecret = _configuration["Razorpay:KeySecret"]!;
        }

        public string CreateOrder(decimal amount, string receiptId)
        {
            RazorpayClient client = new RazorpayClient(_keyId, _keySecret);

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", (int)(amount * 100)); // amount in the smallest currency unit (paise for INR)
            options.Add("receipt", receiptId);
            options.Add("currency", "INR");
            options.Add("payment_capture", "1"); // auto capture

            Razorpay.Api.Order order = client.Order.Create(options);
            return order["id"].ToString();
        }

        public bool VerifyPayment(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            try
            {
                RazorpayClient client = new RazorpayClient(_keyId, _keySecret);

                Dictionary<string, string> attributes = new Dictionary<string, string>();
                attributes.Add("razorpay_order_id", razorpayOrderId);
                attributes.Add("razorpay_payment_id", razorpayPaymentId);
                attributes.Add("razorpay_signature", razorpaySignature);

                Utils.verifyPaymentSignature(attributes);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
