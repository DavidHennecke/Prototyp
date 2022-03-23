syntax = "proto3";

option csharp_namespace = "GrpcServer";

package dataConnector;

service DataConnector {
	rpc ReceiveVectorDataStream (stream VectorDataStream) returns (DataReceivedResponse); 
}

message VectorDataStream{
	oneof content { //carries either the configuration (which input channel to target) or chunks of the actual data
		int32 targetChannel = 1;
		bytes chunk = 2;
	}
}

message DataReceivedResponse {
	int32 finished = 1;
}