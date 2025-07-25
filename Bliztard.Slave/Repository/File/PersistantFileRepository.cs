﻿using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Bliztard.Application.Configurations;
using Bliztard.Application.Extension;
using Bliztard.Slave.BackgroundService;

namespace Bliztard.Slave.Repository.File;

public class PersistantFileRepository(ILogger<PersistantFileRepository> logger, WiwiwiBackgroundService wiwiwiBackgroundService) : IFileRepository
{
    // ReSharper disable once ConvertToConstant.Local
    private readonly string m_DefaultIndex = "primary_index";

    private readonly ILogger<PersistantFileRepository>          m_Logger                  = logger;
    private readonly ConcurrentDictionary<string, MemoryStream> m_SessionContent          = new();
    private readonly WiwiwiBackgroundService                    m_WiwiwiBackgroundService = wiwiwiBackgroundService;
    private readonly ManualResetEventSlim                       m_SavingEventLock         = new(true);

    public Stream CreateStream(string path)
    {
        return m_SessionContent[path] = new MemoryStream();
    }

    public async Task<bool> Save(Guid pathId, string resource, Stream content)
    {
        var contentString = content.ReadContent(Encoding.UTF8);

        m_SavingEventLock.Wait();

        m_WiwiwiBackgroundService.CountCoordinator.Increase();

        var result = await m_WiwiwiBackgroundService.LogTable.LogCreateAction(pathId, resource, contentString) &&
                     m_WiwiwiBackgroundService.WiwiwiTable.Add(pathId, m_DefaultIndex, resource, contentString);

        m_WiwiwiBackgroundService.CountCoordinator.Decrease();

        return result;
    }

    public async Task<bool> Update(Guid pathId, string resource, Stream content)
    {
        var contentString = content.ReadContent(Encoding.UTF8);

        m_SavingEventLock.Wait();

        m_WiwiwiBackgroundService.CountCoordinator.Increase();

        var result = await m_WiwiwiBackgroundService.LogTable.LogUpdateAction(pathId, resource, contentString) && m_WiwiwiBackgroundService.WiwiwiTable.Update(pathId, contentString);

        m_WiwiwiBackgroundService.CountCoordinator.Decrease();
        
        return result;
    }

    public async Task<bool> Rename(string oldResource, string newResource)
    {
        m_SavingEventLock.Wait();

        m_WiwiwiBackgroundService.CountCoordinator.Increase();

        var result = await m_WiwiwiBackgroundService.LogTable.LogRenameAction(Guid.Empty, m_DefaultIndex) && m_WiwiwiBackgroundService.WiwiwiTable.Rename(m_DefaultIndex, oldResource, newResource);

        m_WiwiwiBackgroundService.CountCoordinator.Decrease();
        
        return result;
    }        

    public async Task<bool> Delete(string resource, Guid pathId)
    {
        m_SavingEventLock.Wait();
        
        m_WiwiwiBackgroundService.CountCoordinator.Increase();

        var result = await m_WiwiwiBackgroundService.LogTable.LogDeleteAction(pathId, resource) && m_WiwiwiBackgroundService.WiwiwiTable.Remove(pathId, m_DefaultIndex, resource);

        m_WiwiwiBackgroundService.CountCoordinator.Decrease();
        
        return result;
    }

    public async Task PersistWiwiwiTable()
    {
        m_SavingEventLock.Reset();

        await m_WiwiwiBackgroundService.PersistTable(m_SavingEventLock);
    }

    public bool TryRemoveSessionContent(Guid pathId, [MaybeNullWhen(false)] out Stream content)
    {
        content = null;

        if (!m_SessionContent.TryRemove(pathId.ToString(), out var streamContent))
            return false;

        content = streamContent;

        return true;
    }

    public Stream? Load(string resource)
    {
        m_SavingEventLock.Wait();

        m_WiwiwiBackgroundService.CountCoordinator.Increase();

        var result = m_WiwiwiBackgroundService.FindResource(resource);
        
        m_WiwiwiBackgroundService.CountCoordinator.Decrease();
        
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Resource: {Resource} | Size: {Size} | Load Resource", DateTime.Now, resource, result?.Size() ?? 0);
        
        return result is null ? null : new MemoryStream(Encoding.UTF8.GetBytes(result));
    }

    public void Stats()
    {
        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Start Index Statistics", DateTime.Now);

        foreach (var indexDataSegmentEntry in m_WiwiwiBackgroundService.WiwiwiTable.IndexTable.DataSegment.GetEntries(m_DefaultIndex))
            m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Key: {Key} | Value: {Value} Index Statistics", DateTime.Now, indexDataSegmentEntry.IndexKey.ToString(),
                              indexDataSegmentEntry.IndexValue.value);

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | End Index Statistics", DateTime.Now);

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | Start Record Statistics", DateTime.Now);

        foreach (var recordKeySegmentEntry in m_WiwiwiBackgroundService.WiwiwiTable.RecordTable.KeySegment.GetEntries())
            m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | PathId: {PathId?} | Status: {} | Record Statistics", DateTime.Now, recordKeySegmentEntry.RecordGuid.value,
                              recordKeySegmentEntry.RecordOffset == -1 ? "Removed" : "Present");

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | End Record Statistics", DateTime.Now);

        m_Logger.LogDebug("Timestamp: {Timestamp:HH:mm:ss.ffffff} | End Log Statistics", DateTime.Now);
    }

    public bool IsBlockFull()
    {
        return m_WiwiwiBackgroundService.WiwiwiTable.RecordTable.Size() >= Configuration.File.BlockSize;
    }
}
