﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using CollAction.Helpers;

namespace CollAction.Services
{
    public class NewsletterSubscriptionService : INewsletterSubscriptionService
    {
        private static MailChimpManager manager;
        private static string newsletterListId;

        public NewsletterSubscriptionService(IOptions<NewsletterSubscriptionServiceOptions> options)
        {
            manager = new MailChimpManager(options.Value.MailChimpKey);
            newsletterListId = options.Value.MailChimpNewsletterListId;
        }

        public async Task<bool> IsSubscribedAsync(string email)
        {
            MailChimpManager.SubscriptionStatus status = await manager.GetListMemberStatusAsync(newsletterListId, email);
            return status == MailChimpManager.SubscriptionStatus.Pending || status == MailChimpManager.SubscriptionStatus.Subscribed;
        }

        public async Task SetSubscriptionAsync(string email, bool wantsSubscription, bool requireEmailConfirmationIfSubscribing)
        {
            if (wantsSubscription)
            {
                await manager.AddOrUpdateListMemberAsync(newsletterListId, email, requireEmailConfirmationIfSubscribing);
            }
            else
            {
                await manager.DeleteListMemberAsync(newsletterListId, email);
            }
        }
    }
}