﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Data.DataAccessLayer {
	public static class DBExtensions {
		public static EntityEntry<TEnt> AddIfNotExists<TEnt, TKey>(this DbSet<TEnt> dbSet, TEnt entity) where TEnt : class {
			var exists = dbSet.Any(c => c == entity);
			return exists ? null : dbSet.Add(entity);
		}

		public static IEnumerable<TEnt> FindNewEntities<TEnt, TKey>(this IEnumerable<TEnt> current, IEnumerable<TEnt> source, Func<TEnt, TKey> keySelector) where TEnt : class {
			var existingKeys = current.Select(keySelector).ToHashSet();
			return source.Where(e => !existingKeys.Contains(keySelector(e)));
		}

		public static async Task AddRangeIfNotExists<TEnt, TKey>(this DbSet<TEnt> dbSet, IEnumerable<TEnt> entities, Func<TEnt, TKey> keySelector) where TEnt : class {
			var newEntities = FindNewEntities(dbSet, entities, keySelector);
			await dbSet.AddRangeAsync(newEntities);
		}
	}
}