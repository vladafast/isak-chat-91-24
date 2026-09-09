using IsakChat.App.Models;
using SQLite;

namespace IsakChat.App.Services;

// Tanak omotac oko sqlite-net-pcl konekcije. Registrovan kao singleton u MauiProgram.
public class LocalDatabase
{
    private readonly SQLiteAsyncConnection _connection;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public LocalDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "isakchat_local.db3");
        _connection = new SQLiteAsyncConnection(dbPath);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;
            await _connection.CreateTableAsync<LocalSession>();
            await _connection.CreateTableAsync<LocalMessage>();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<LocalSession?> GetSessionAsync()
    {
        await EnsureInitializedAsync();
        return await _connection.Table<LocalSession>().FirstOrDefaultAsync();
    }

    public async Task SaveSessionAsync(LocalSession session)
    {
        await EnsureInitializedAsync();
        session.Id = 1;
        await _connection.InsertOrReplaceAsync(session);
    }

    public async Task ClearSessionAsync()
    {
        await EnsureInitializedAsync();
        await _connection.Table<LocalSession>().DeleteAsync(s => s.Id == 1);
    }

    public async Task<List<LocalMessage>> GetMessagesAsync(string conversationKey)
    {
        await EnsureInitializedAsync();
        return await _connection.Table<LocalMessage>()
            .Where(m => m.ConversationKey == conversationKey)
            .OrderBy(m => m.ServerId)
            .ToListAsync();
    }

    public async Task<int> GetMaxServerIdAsync(string conversationKey)
    {
        await EnsureInitializedAsync();
        var messages = await _connection.Table<LocalMessage>()
            .Where(m => m.ConversationKey == conversationKey)
            .ToListAsync();
        return messages.Count == 0 ? 0 : messages.Max(m => m.ServerId);
    }

    public async Task SaveMessagesAsync(IEnumerable<LocalMessage> messages)
    {
        await EnsureInitializedAsync();
        await _connection.InsertAllAsync(messages, "OR REPLACE");
    }

    public async Task MarkDeletedAsync(string conversationKey, int serverId)
    {
        await EnsureInitializedAsync();
        var existing = await _connection.Table<LocalMessage>()
            .Where(m => m.ConversationKey == conversationKey && m.ServerId == serverId)
            .FirstOrDefaultAsync();
        if (existing is null) return;

        existing.IsDeleted = true;
        existing.Text = "🚫 Poruka je obrisana";
        existing.ImageUrl = null;
        await _connection.UpdateAsync(existing);
    }

    public async Task ClearAllDataAsync()
    {
        await EnsureInitializedAsync();
        await _connection.DeleteAllAsync<LocalMessage>();
        await _connection.DeleteAllAsync<LocalSession>();
    }
}
