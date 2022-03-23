﻿syntax = "proto3";

option csharp_namespace = "GrpcServer";

package controlConnector;
import "google/protobuf/struct.proto";

service ControlConnector {
	rpc RunProcess (RunRequest) returns (stream RunUpdate);
	rpc SendData (ChannelInfo) returns (SendResponse);
	rpc SetLayer (stream ByteStream) returns (LayerResponse);
	rpc GetLayer (LayerRequest) returns (stream ByteStream);

	rpc StartUI (UIRequest) returns (UIResponse);
	rpc SendSettings (Settings) returns (SettingsResponse);
}

message ChannelInfo {
	int32 sourceChannelID = 1;
	string targetNodeUrl = 2;
	int32 targetChannelID = 3;
}

message RunRequest {
}

message RunUpdate {
	int32 progress = 1;
}

message SendResponse {
	int32 finished = 1;
}

message ByteStream {
	bytes chunk = 1;
}

message LayerResponse {
	int32 finished = 1;
}

message LayerRequest {
	int32 outputChannelID = 1;
}

message UIRequest{
	string params = 1;
}

message UIResponse {
	int32 success = 1;
}

message Settings {
	google.protobuf.Struct mapping = 1;
}

message SettingsResponse{
}