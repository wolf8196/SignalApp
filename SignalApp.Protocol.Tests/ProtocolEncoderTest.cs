using System;
using System.Buffers;
using FluentAssertions;
using Xunit;

namespace SignalApp.Protocol.Tests
{
    public class ProtocolEncoderTest
    {
        private readonly ProtocolEncoder target;

        public ProtocolEncoderTest()
        {
            target = new ProtocolEncoder();
        }

        [Theory]
        [MemberData(nameof(ParsesHeaderTestData))]
        public void DecodesHeader(byte[] input, ProtocolRecord expected)
        {
            var actual = target.DecodeHeader(new ReadOnlySequence<byte>(input));
            actual.Should().BeEquivalentTo(expected);
        }

        public static TheoryData<byte[], ProtocolRecord> ParsesHeaderTestData()
        {
            return new TheoryData<byte[], ProtocolRecord>
            {
                { [0b_00001000, 0b_000_00000, 123, 255], new ProtocolRecord { Type = 0, TotalLength = 8    } }, // 000 (type 0) 00001000 (8 length)
                { [0b_00011110, 0b_000_00000, 123, 255], new ProtocolRecord { Type = 0, TotalLength = 30  } }, // 000 (type 0) 0000100101100 (300 length)
                { [0b_00101100, 0b_100_00001, 123, 255], new ProtocolRecord { Type = 4, TotalLength = 300  } }, // 100 (type 4) 0000100101100 (300 length)
                { [0b_00101100, 0b_001_01001, 123, 255], new ProtocolRecord { Type = 1, TotalLength = 2348 } }, // 100 (type 1) 0100100101100 (2348 length)
                { [0b_00001100, 0b_010_00001, 123, 255], new ProtocolRecord { Type = 2, TotalLength = 268  } }, // 100 (type 2) 0000100001100 (268 length)
                { [0b_00101000, 0b_111_10001, 123, 255], new ProtocolRecord { Type = 7, TotalLength = 4392 } }, // 100 (type 7) 1000100101000 (4392 length)
            };
        }

        [Theory]
        [MemberData(nameof(DecodesDataTestData))]
        public void DecodesData(byte[] input, ProtocolRecord expected)
        {
            var actual = target.DecodeData(new ReadOnlySequence<byte>(input));
            actual.Should().BeEquivalentTo(
                expected,
                opts => opts.Using<DateTime>(ctx =>
                {
                    ctx.Subject.Should().Be(ctx.Expectation);
                    ctx.Subject.Kind.Should().Be(ctx.Expectation.Kind);
                })
                .WhenTypeIs<DateTime>());
        }

        public static TheoryData<byte[], ProtocolRecord> DecodesDataTestData()
        {
            return new TheoryData<byte[], ProtocolRecord>
            {
                {
                    [
                        0xB0, 0x85, 0x9C, 0x69, 0x00, 0x00, 0x00, 0x00, // Timestamp = 1771865520 (2026-02-23 16:52:00 UTC)
                        0x80, 0xCF, 0x2C, 0x06, 0x00, 0x00, 0x00, 0x00, // Frequency = 103_600_000 Hz (103.6 MHz)
                        0xD4, 0x30, 0x00, 0x00, // Bandwidth = 12_500 Hz (12.5 kHz)
                        0xB8, 0x1E, 0x85, 0xEB, 0x51, 0x78, 0x2E, 0x40 // SNR = 15.235
                    ],
                    new ProtocolRecord
                    {
                        Timestamp = new DateTime(2026, 2, 23, 16, 52, 0, DateTimeKind.Utc),
                        Frequency = 103_600_000UL,
                        Bandwidth = 12_500u,
                        SignalNoiseRatio = 15.235
                    }
                },
                {
                    [
                        0xA0, 0x53, 0x9C, 0x69, 0x00, 0x00, 0x00, 0x00, // Timestamp = 1771852704 (2026-02-23 13:18:24 UTC)
                        0x80, 0xCF, 0x2C, 0x06, 0x00, 0x00, 0x00, 0x00, // Frequency = 103_600_000 Hz (103.6 MHz)
                        0xD4, 0x30, 0x00, 0x00, // Bandwidth = 12_500 Hz (12.5 kHz)
                        0x3F, 0x35, 0x5E, 0xBA, 0x49, 0x4C, 0x2D, 0x40 // SNR = 14.649
                    ],
                    new ProtocolRecord
                    {
                        Timestamp = new DateTime(2026, 2, 23, 13, 18, 24, DateTimeKind.Utc),
                        Frequency = 103_600_000UL,
                        Bandwidth = 12_500u,
                        SignalNoiseRatio = 14.649
                    }
                },
                {
                    [
                        0x4D, 0x53, 0x9C, 0x69, 0x00, 0x00, 0x00, 0x00, // Timestamp = 1771852621 (2026-02-23 13:17:01 UTC)
                        0x40, 0xF7, 0x35, 0x06, 0x00, 0x00, 0x00, 0x00, // Frequency = 104_200_000 Hz (104.2 MHz)
                        0xC6, 0xA6, 0x01, 0x00, // Bandwidth = 108_230 Hz (108.23 kHz)
                        0x27, 0x31, 0x08, 0xAC, 0x1C, 0x9A, 0x2D, 0x40 // SNR = 14.801
                    ],
                    new ProtocolRecord
                    {
                        Timestamp = new DateTime(2026, 2, 23, 13, 17, 1, DateTimeKind.Utc),
                        Frequency = 104_200_000UL,
                        Bandwidth = 108_230u,
                        SignalNoiseRatio = 14.801
                    }
                },
            };
        }

        [Theory]
        [MemberData(nameof(EncodesRecordTestData))]
        public void EncodesRecord(ProtocolRecord record, byte[] expected)
        {
            var actual = target.Encode(record);
            actual.Should().BeEquivalentTo(expected);
        }

        public static TheoryData<ProtocolRecord, byte[]> EncodesRecordTestData()
        {
            return new TheoryData<ProtocolRecord, byte[]>
            {
                {
                    new ProtocolRecord
                    {
                        TotalLength = 30,
                        Type = 0,
                        Timestamp = new DateTime(2026, 2, 23, 16, 52, 0, DateTimeKind.Utc),
                        Frequency = 103_600_000UL,
                        Bandwidth = 12_500u,
                        SignalNoiseRatio = 15.235
                    },
                    [
                        0b_00011110, 0b_000_00000,
                        0xB0, 0x85, 0x9C, 0x69, 0x00, 0x00, 0x00, 0x00, // Timestamp = 1771865520 (2026-02-23 16:52:00 UTC)
                        0x80, 0xCF, 0x2C, 0x06, 0x00, 0x00, 0x00, 0x00, // Frequency = 103_600_000 Hz (103.6 MHz)
                        0xD4, 0x30, 0x00, 0x00, // Bandwidth = 12_500 Hz (12.5 kHz)
                        0xB8, 0x1E, 0x85, 0xEB, 0x51, 0x78, 0x2E, 0x40 // SNR = 15.235
                    ]
                },
                {
                    new ProtocolRecord
                    {
                        TotalLength = 30,
                        Type = 0,
                        Timestamp = new DateTime(2026, 2, 23, 13, 18, 24, DateTimeKind.Utc),
                        Frequency = 103_600_000UL,
                        Bandwidth = 12_500u,
                        SignalNoiseRatio = 14.649
                    },
                    [
                        0b_00011110, 0b_000_00000,
                        0xA0, 0x53, 0x9C, 0x69, 0x00, 0x00, 0x00, 0x00, // Timestamp = 1771852704 (2026-02-23 13:18:24 UTC)
                        0x80, 0xCF, 0x2C, 0x06, 0x00, 0x00, 0x00, 0x00, // Frequency = 103_600_000 Hz (103.6 MHz)
                        0xD4, 0x30, 0x00, 0x00, // Bandwidth = 12_500 Hz (12.5 kHz)
                        0x3F, 0x35, 0x5E, 0xBA, 0x49, 0x4C, 0x2D, 0x40 // SNR = 14.649
                    ]
                },
                {
                    new ProtocolRecord
                    {
                        TotalLength = 30,
                        Type = 0,
                        Timestamp = new DateTime(2026, 2, 23, 13, 17, 1, DateTimeKind.Utc),
                        Frequency = 104_200_000UL,
                        Bandwidth = 108_230u,
                        SignalNoiseRatio = 14.801
                    },
                    [
                        0b_00011110, 0b_000_00000,
                        0x4D, 0x53, 0x9C, 0x69, 0x00, 0x00, 0x00, 0x00, // Timestamp = 1771852621 (2026-02-23 13:17:01 UTC)
                        0x40, 0xF7, 0x35, 0x06, 0x00, 0x00, 0x00, 0x00, // Frequency = 104_200_000 Hz (104.2 MHz)
                        0xC6, 0xA6, 0x01, 0x00, // Bandwidth = 108_230 Hz (108.23 kHz)
                        0x27, 0x31, 0x08, 0xAC, 0x1C, 0x9A, 0x2D, 0x40 // SNR = 14.801
                    ]
                },
            };
        }
    }
}