using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network;

public struct OutgoingMessage
{
	private byte messageType;

	private long farmerID;

	private object[] data;

	public byte MessageType => messageType;

	public long FarmerID => farmerID;

	public Farmer SourceFarmer => Game1.getFarmer(farmerID);

	public ReadOnlyCollection<object> Data => Array.AsReadOnly(data);

	public OutgoingMessage(byte messageType, long farmerID, params object[] data)
	{
		this.messageType = messageType;
		this.farmerID = farmerID;
		this.data = data;
	}

	public OutgoingMessage(byte messageType, Farmer sourceFarmer, params object[] data)
		: this(messageType, sourceFarmer.UniqueMultiplayerID, data)
	{
	}

	public OutgoingMessage(IncomingMessage message)
		: this(message.MessageType, message.FarmerID, message.Data)
	{
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(messageType);
		writer.Write(farmerID);
		object[] data = this.data;
		writer.WriteSkippable(delegate
		{
			object[] array = data;
			foreach (object obj in array)
			{
				if (!(obj is Vector2 vector))
				{
					if (!(obj is Guid guid))
					{
						if (!(obj is byte[] buffer))
						{
							if (!(obj is bool flag))
							{
								if (!(obj is byte value))
								{
									if (!(obj is int value2))
									{
										if (!(obj is short value3))
										{
											if (!(obj is float value4))
											{
												if (!(obj is long value5))
												{
													if (!(obj is string value6))
													{
														if (!(obj is string[] array2))
														{
															if (!(obj is IConvertible))
															{
																throw new InvalidDataException();
															}
															if (!obj.GetType().IsValueType)
															{
																throw new InvalidDataException();
															}
															writer.WriteEnum(obj);
														}
														else
														{
															writer.Write((byte)array2.Length);
															for (int j = 0; j < array2.Length; j++)
															{
																writer.Write(array2[j]);
															}
														}
													}
													else
													{
														writer.Write(value6);
													}
												}
												else
												{
													writer.Write(value5);
												}
											}
											else
											{
												writer.Write(value4);
											}
										}
										else
										{
											writer.Write(value3);
										}
									}
									else
									{
										writer.Write(value2);
									}
								}
								else
								{
									writer.Write(value);
								}
							}
							else
							{
								writer.Write((byte)(flag ? 1u : 0u));
							}
						}
						else
						{
							writer.Write(buffer);
						}
					}
					else
					{
						writer.Write(guid.ToByteArray());
					}
				}
				else
				{
					writer.Write(vector.X);
					writer.Write(vector.Y);
				}
			}
		});
	}
}
