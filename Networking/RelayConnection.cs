using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;

public class RelayConnection
{

    NetworkManager sdp_NetworkManager;
    Lobby SDP_Lobby;
    int max_connected_players = 4;
    string join_code;

    public RelayConnection(ref NetworkManager incoming_NetworkManager)
    {
        sdp_NetworkManager = incoming_NetworkManager;
    }

    async void AllocateRelay()
    {

        Allocation host_allocation = await RelayService.Instance.CreateAllocationAsync(max_connected_players, region: null);
        join_code = await RelayService.Instance.GetJoinCodeAsync(host_allocation.AllocationId);

        var utp = (UnityTransport)sdp_NetworkManager.NetworkConfig.NetworkTransport;
        utp.SetRelayServerData(new RelayServerData(host_allocation, "dtls"));

    }

    async Task<(bool Success, Lobby Lobby)> CreateLobby(string player_id, string lobby_name, int max_players)
    {

        CreateLobbyOptions createOptions = new CreateLobbyOptions
        {
            IsPrivate = false,
            IsLocked = true, // locking the lobby at creation to prevent other players from joining before it is ready
            Player = new Player(id: player_id, data: null),
            //Data = lobbyData
        };

        var lobby = await LobbyService.Instance.CreateLobbyAsync(lobby_name, max_players, createOptions);
        return (true, lobby);

    }

    public async void TryCreateLobbyAsync()
    {
        var lobby_attempt = await CreateLobby(AuthenticationService.Instance.PlayerId, "CapstanServer", 4);

        if (lobby_attempt.Success)
        {

        }
    }
}
