//
//  CachingDiscordRestInviteAPI.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Results;
using Remora.Discord.Caching.Services;
using Remora.Discord.Core;
using Remora.Discord.Rest;
using Remora.Discord.Rest.API;
using Remora.Discord.Rest.Results;

namespace Remora.Discord.Caching.API
{
    /// <inheritdoc />
    public class CachingDiscordRestInviteAPI : DiscordRestInviteAPI
    {
        private readonly CacheService _cacheService;

        /// <inheritdoc cref="DiscordRestInviteAPI(DiscordHttpClient)" />
        public CachingDiscordRestInviteAPI
        (
            DiscordHttpClient discordHttpClient,
            CacheService cacheService
        )
            : base(discordHttpClient)
        {
            _cacheService = cacheService;
        }

        /// <inheritdoc />
        public override async Task<IRetrieveRestEntityResult<IInvite>> GetInviteAsync
        (
            string inviteCode,
            Optional<bool> withCounts = default,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateInviteCacheKey(inviteCode);
            if (_cacheService.TryGetValue<IInvite>(key, out var cachedInstance))
            {
                return RetrieveRestEntityResult<IInvite>.FromSuccess(cachedInstance);
            }

            var getInvite = await base.GetInviteAsync(inviteCode, withCounts, ct);
            if (!getInvite.IsSuccess)
            {
                return getInvite;
            }

            var invite = getInvite.Entity;
            _cacheService.Cache(key, invite);

            return getInvite;
        }

        /// <inheritdoc />
        public override async Task<IDeleteRestEntityResult<IInvite>> DeleteInviteAsync
        (
            string inviteCode,
            CancellationToken ct = default
        )
        {
            var deleteInvite = await base.DeleteInviteAsync(inviteCode, ct);
            if (!deleteInvite.IsSuccess)
            {
                return deleteInvite;
            }

            var key = KeyHelpers.CreateInviteCacheKey(inviteCode);
            _cacheService.Evict(key);

            return deleteInvite;
        }
    }
}
