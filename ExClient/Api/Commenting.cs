﻿using ExClient.Galleries.Commenting;
using Newtonsoft.Json;
using System;

namespace ExClient.Api
{
    internal abstract class CommentRequest : GalleryRequest
    {
        public CommentRequest(Comment comment)
            : base(comment.Owner.Owner)
        {
            var gallery = comment.Owner.Owner;
            this.Id = comment.ID;
        }

        [JsonProperty("comment_id")]
        public int Id { get; }
    }

    internal sealed class CommentVoteRequest : CommentRequest, IRequestOf<CommentVoteRequest, CommentVoteResponse>
    {
        public CommentVoteRequest(Comment comment, VoteState vote)
            : base(comment)
        {
            if (vote != VoteState.Down && vote != VoteState.Up)
                throw new ArgumentOutOfRangeException(nameof(vote));
            this.Vote = vote;
        }

        public override string Method => "votecomment";

        [JsonProperty("comment_vote")]
        public VoteState Vote { get; }
    }

    internal sealed class CommentEditRequest : CommentRequest, IRequestOf<CommentEditRequest, CommentEditResponse>
    {
        public CommentEditRequest(Comment comment)
            : base(comment)
        {
        }

        public override string Method => "geteditcomment";
    }

    internal class CommentResponse : ApiResponse
    {
        [JsonProperty("comment_id")]
        public int ID { get; set; }

    }

    internal sealed class CommentVoteResponse : CommentResponse, IResponseOf<CommentVoteRequest, CommentVoteResponse>
    {
        [JsonProperty("comment_score")]
        public int Score { get; set; }
        [JsonProperty("comment_vote")]
        public VoteState Vote { get; set; }
    }

    internal sealed class CommentEditResponse : CommentResponse, IResponseOf<CommentEditRequest, CommentEditResponse>
    {
        [JsonProperty("editable_comment")]
        public string Editable { get; set; }
    }
}
