using Myth.Interfaces;

namespace Myth.Mapper.Test {
	internal class OriginForMapTo : IMapTo<OriginForMapTo, DestinationForMapTo> {
		public int OriginId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }

		public void MapTo( MappingBuilder<OriginForMapTo, DestinationForMapTo> builder ) {
			builder
				.ForMember(
					src => src.Enabled,
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					src => src.OriginId,
					dest => dest.DestinationId,
					( src, sp ) => src.OriginId )
				.AutoMapRemaining( );
		}
	}

	internal class DestinationForMapTo : IMapFrom<DestinationForMapTo, OriginForMapTo> {
		public int DestinationId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }

		public void MapFrom( MappingBuilder<DestinationForMapTo, OriginForMapTo> builder ) {
			builder
				.ForMember(
					src => src.Enabled,
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					src => src.DestinationId,
					dest => dest.OriginId,
					( src, sp ) => src.DestinationId )
				.AutoMapRemaining( );
		}

	}

	internal class OriginForMapFrom{
		public int OriginId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }

	}

	internal class DestinationForMapFrom : IMapFrom<DestinationForMapFrom, OriginForMapFrom> {
		public int DestinationId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }

		public void MapFrom( MappingBuilder<DestinationForMapFrom, OriginForMapFrom> builder ) {
			builder
				.ForMember(
					src => src.Enabled,
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					src => src.DestinationId,
					dest => dest.OriginId,
					( src, sp ) => src.DestinationId )
				.AutoMapRemaining( );
		}

	}
}
